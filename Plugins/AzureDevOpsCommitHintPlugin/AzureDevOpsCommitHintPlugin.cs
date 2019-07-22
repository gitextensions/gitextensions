using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using AzureDevOpsCommitHintPlugin.Properties;
using GitExtUtils.GitUI;
using GitUI;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.UserControls;
using Newtonsoft.Json.Linq;
using ResourceManager;

[assembly: AssemblyDescription("GitExtensions plugin for integration with Azure DevOps backlog")]

namespace AzureDevOpsCommitHintPlugin
{
    [Export(typeof(IGitPlugin))]
    public class AzureDevOpsCommitHintPlugin : GitPluginBase, IGitPluginForRepository
    {
        private static readonly TranslationString AzureDevOpsCredentialsLabel = new TranslationString("Azure DevOps credentials");
        private static readonly TranslationString AzureDevOpsQueryLabel = new TranslationString("Work items Query (WIQL)");
        private static readonly TranslationString MessageTemplateLabel = new TranslationString("Message Template");
        private static readonly TranslationString AzureDevOpsFieldsLabel = new TranslationString("AzureDevOps fields");
        private static readonly TranslationString OpenWiqlDocumentation = new TranslationString("Open documentation for WIQL (Work Item Query Language)");
        private static readonly TranslationString AzureDevopsWiqlEditorExtension = new TranslationString("Azure DevOps Wiql Editor extension (that could help you to write the Wiql query)");
        private static readonly TranslationString PreviewButtonText = new TranslationString("Preview");
        private static readonly TranslationString EmptyQueryResultMessage = new TranslationString("[Empty AzureDevOps Query Result]");
        private static readonly TranslationString FirstTaskPreview = new TranslationString("First Task Preview");
        private static readonly TranslationString Email = new TranslationString("Email");
        private static readonly TranslationString Token = new TranslationString("Token");
        private static readonly TranslationString GenerateToken = new TranslationString("Generate Personal Access Token. Requires scope \"Work items (read)\".");
        private static readonly TranslationString InvalidProjectUrl = new TranslationString("Invalid project url '{0}'\n Expected format: {1}");
        private static readonly TranslationString ParsingResponseError = new TranslationString("Parsing response error");
        private static readonly TranslationString Error = new TranslationString("error");
        private static readonly TranslationString ApiCallError = new TranslationString("Call to api error");
        private static readonly TranslationString WorkItemsFound = new TranslationString("Work item(s) found:");
        private static readonly TranslationString QueryResult = new TranslationString("Query result");
        private static readonly TranslationString WorkItemsFields = new TranslationString("Your custom fields and all systems fields like: ");
        private static readonly TranslationString WorkItemsDocumentationLink = new TranslationString("See work items fields examples in documentation");
        private static readonly TranslationString HowToRetrieveWorkItemsFieldsValues = new TranslationString("Click on preview to gather them");
        private static readonly TranslationString WorkItemsFieldsValuesCaption = new TranslationString("Fields list with values");
        private static readonly TranslationString FieldsMandatory = new TranslationString("Project url, email and token are mandatory fields. Please, fill them.");

        private static readonly List<string> SupportedWorkItemFields = new List<string>
        {
            "System.AreaPath",
            "System.TeamProject",
            "System.IterationPath",
            "System.WorkItemType",
            "System.State",
            "System.Reason",
            "System.CreatedDate",
            "System.ChangedDate",
            "System.Title",
            "System.Description",
            "System.Tags",
            "System.CreatedBy|displayName",
            "System.AssignedTo|displayName",
            "System.ChangedBy|displayName",
            "System.CreatedBy|uniqueName",
            "System.AssignedTo|uniqueName",
            "System.ChangedBy|uniqueName",
        };

        private const string DefaultFormat = "#{id}: {System.Title}\n\n{System.Description}";
        private string _projectUrl;
        private string _query;
        private string _stringTemplate = DefaultFormat;
        private HttpClient _httpClient;

        private readonly BoolSetting _enabledSettings = new BoolSetting("Enabled", false);
        private readonly StringSetting _projectUrlSettings = new StringSetting("AzureDevOps project URL", @"https://dev.azure.com/{organization}/{project}");
        private readonly CredentialsSetting _credentialsSettings;
        private readonly StringSetting _wiqlQuerySettings = new StringSetting("WIQL Query", AzureDevOpsQueryLabel.Text, "Select [System.Id] From WorkItems Where ( [System.WorkItemType] = 'Product Backlog Item' OR [System.WorkItemType] = 'Bug' ) AND [State] <> 'Closed' AND [State] <> 'Removed' AND [System.AssignedTo] = @Me AND [System.IterationPath] = @CurrentIteration order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc", true);
        private readonly StringSetting _stringTemplateSetting = new StringSetting("AzureDevOps Message Template", MessageTemplateLabel.Text, DefaultFormat, true);
        private readonly PseudoSetting _allFieldsAndValuesSetting = new PseudoSetting(HowToRetrieveWorkItemsFieldsValues.Text, WorkItemsFieldsValuesCaption.Text, DpiUtil.Scale(200), t => t.ScrollBars = ScrollBars.Both);

        private IGitModule _gitModule;
        private IList<CommitTemplate> _currentCommitTemplates;
        private Button _btnPreview;
        private string _allFieldsAndValues;

        public AzureDevOpsCommitHintPlugin() : base(true)
        {
            SetNameAndDescription("AzureDevOps Commit Hint");
            Translate();
            Icon = Resources.IconAzureDevOps;

            _credentialsSettings = new CredentialsSetting("AzureDevOpsCredentials", AzureDevOpsCredentialsLabel.Text, () => _gitModule?.WorkingDir);
        }

        public override bool Execute(GitUIEventArgs args)
        {
            if (!_enabledSettings.ValueOrDefault(Settings) || _httpClient == null)
            {
                args.GitUICommands.StartSettingsDialog(this);
                return false;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await DisplayQueryResultAsync(_httpClient, _projectUrl, _query, _stringTemplate);
                });

            return false;
        }

        private HttpClient GetHttpClient(string login, string password)
        {
            var httpClient = new HttpClient();
            var base64Authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{login}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Authorization);
            return httpClient;
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return _enabledSettings;

            yield return _projectUrlSettings;

            _credentialsSettings.CustomControl = new CredentialsControl(Email.Text, Token.Text);
            yield return _credentialsSettings;

            var generateTokenLink = new LinkLabel { Text = GenerateToken.Text, Tag = "https://dev.azure.com/{organization}/_usersSettings/tokens" };
            generateTokenLink.Click += GenerateTokenLink_Click;
            yield return new PseudoSetting(generateTokenLink);

            yield return _wiqlQuerySettings;

            var documentationLink = new LinkLabelOpener { Text = OpenWiqlDocumentation.Text, Tag = "https://docs.microsoft.com/azure/devops/boards/queries/wiql-syntax" };
            yield return new PseudoSetting(documentationLink);

            var azureDevopsWiqlEditorExtensionLink = new LinkLabelOpener { Text = AzureDevopsWiqlEditorExtension.Text, Tag = "https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor" };
            yield return new PseudoSetting(azureDevopsWiqlEditorExtensionLink);

            yield return new PseudoSetting($"{WorkItemsFields}: {{id}} {{{string.Join("} {", SupportedWorkItemFields.OrderBy(i => i).ToArray())}}}",
                AzureDevOpsFieldsLabel.Text, DpiUtil.Scale(55));

            var workItemsFieldsHelpLink = new PseudoSetting(new LinkLabelOpener { Text = WorkItemsDocumentationLink.Text, Tag = "https://docs.microsoft.com/rest/api/azure/devops/wit/work%20items/get%20work%20item#examples" });
            yield return workItemsFieldsHelpLink;

            var txtTemplate = new TextBox
            {
                Height = DpiUtil.Scale(130),
                Multiline = true,
                ScrollBars = ScrollBars.Horizontal
            };

            _btnPreview = new Button
            {
                Text = PreviewButtonText.Text,
                Top = txtTemplate.Height - DpiUtil.Scale(30),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            _btnPreview.Size = DpiUtil.Scale(_btnPreview.Size);
            _btnPreview.Click += btnPreviewClick;
            _btnPreview.Left = txtTemplate.Width - _btnPreview.Width - DpiUtil.Scale(8);
            txtTemplate.Controls.Add(_btnPreview);
            _stringTemplateSetting.CustomControl = txtTemplate;
            yield return _stringTemplateSetting;

            yield return _allFieldsAndValuesSetting;
        }

        private void btnPreviewClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var projectUrl = _projectUrlSettings.CustomControl.Text;
                if (string.IsNullOrWhiteSpace(projectUrl)
                    || string.IsNullOrWhiteSpace(_credentialsSettings.CustomControl.UserName)
                    || string.IsNullOrWhiteSpace(_credentialsSettings.CustomControl.Password))
                {
                    MessageBox.Show(FieldsMandatory.Text);
                    return;
                }

                _btnPreview.Enabled = false;

                var localQuery = _wiqlQuerySettings.CustomControl.Text;
                var localStringTemplate = _stringTemplateSetting.CustomControl.Text;

                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        using (var httpClient = GetHttpClient(_credentialsSettings.CustomControl.UserName,
                            _credentialsSettings.CustomControl.Password))
                        {
                            _allFieldsAndValues = null;
                            await DisplayQueryResultAsync(httpClient, projectUrl, localQuery, localStringTemplate);

                            if (_allFieldsAndValues != null)
                            {
                                _allFieldsAndValuesSetting.CustomControl.Text = _allFieldsAndValues;
                            }

                            _btnPreview.Enabled = true;
                        }
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                _btnPreview.Enabled = true;
            }
        }

        private void GenerateTokenLink_Click(object sender, EventArgs e)
        {
            var projectUrl = _projectUrlSettings.CustomControl.Text;
            if (string.IsNullOrWhiteSpace(projectUrl))
            {
                MessageBox.Show(string.Format(InvalidProjectUrl.Text, projectUrl, _projectUrlSettings.DefaultValue));
                return;
            }

            var urlParts = projectUrl.Split('/');
            if (urlParts.Length < 4)
            {
                MessageBox.Show(string.Format(InvalidProjectUrl.Text, projectUrl, _projectUrlSettings.DefaultValue));
                return;
            }

            var url = (string)((LinkLabel)sender).Tag;
            Process.Start(url.Replace("{organization}", urlParts[3]));
        }

        private async Task DisplayQueryResultAsync(HttpClient httpClient, string projectUrl, string wiqlQuery, string templateString)
        {
            var commitTemplates = await GetCommitTemplatesAsync(httpClient, projectUrl, wiqlQuery,
                    templateString).ConfigureAwait(true);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string preview;
            if (commitTemplates.Any())
            {
                preview = $"{WorkItemsFound}{Environment.NewLine}" + string.Join(Environment.NewLine, commitTemplates.Select(jt => jt.Title))
                          + $"{Environment.NewLine}{Environment.NewLine}---------------{Environment.NewLine}{FirstTaskPreview}{Environment.NewLine}{Environment.NewLine}"
                          + commitTemplates.First().Text;
            }
            else
            {
                preview = EmptyQueryResultMessage.Text;
            }

            MessageBox.Show(null, preview, QueryResult.Text);
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            base.Register(gitUiCommands);
            _gitModule = gitUiCommands.GitModule;
            gitUiCommands.PostSettings += gitUiCommands_PostSettings;
            gitUiCommands.PreCommit += gitUiCommands_PreCommit;
            gitUiCommands.PostCommit += gitUiCommands_PostRepositoryChanged;
            gitUiCommands.PostRepositoryChanged += gitUiCommands_PostRepositoryChanged;
            UpdateAzureDevOpsSettings();
        }

        private void UpdateAzureDevOpsSettings()
        {
            if (!_enabledSettings.ValueOrDefault(Settings))
            {
                return;
            }

            _projectUrl = _projectUrlSettings.ValueOrDefault(Settings);
            var credentials = _credentialsSettings.GetValueOrDefault(Settings);

            if (string.IsNullOrWhiteSpace(_projectUrl)
                || string.IsNullOrWhiteSpace(credentials.UserName)
                || string.IsNullOrWhiteSpace(credentials.Password))
            {
                return;
            }

            _httpClient?.Dispose();

            _httpClient = GetHttpClient(credentials.UserName, credentials.Password);

            _query = _wiqlQuerySettings.ValueOrDefault(Settings);
            _stringTemplate = _stringTemplateSetting.ValueOrDefault(Settings);
            if (_btnPreview == null)
            {
                return;
            }

            _btnPreview.Click -= btnPreviewClick;
            _btnPreview = null;
        }

        private void gitUiCommands_PostSettings(object sender, GitUIPostActionEventArgs e)
        {
            UpdateAzureDevOpsSettings();
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            base.Unregister(gitUiCommands);
            _httpClient?.Dispose();
            gitUiCommands.PreCommit -= gitUiCommands_PreCommit;
            gitUiCommands.PostCommit -= gitUiCommands_PostRepositoryChanged;
            gitUiCommands.PostSettings -= gitUiCommands_PostSettings;
            gitUiCommands.PostRepositoryChanged -= gitUiCommands_PostRepositoryChanged;
        }

        private void gitUiCommands_PreCommit(object sender, GitUIEventArgs e)
        {
            if (!_enabledSettings.ValueOrDefault(Settings))
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                _currentCommitTemplates = await GetCommitTemplatesAsync(_httpClient, _projectUrlSettings.ValueOrDefault(Settings), _query, _stringTemplate);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                foreach (var commitTemplate in _currentCommitTemplates)
                {
                    e.GitUICommands.AddCommitTemplate(commitTemplate.Title, () => commitTemplate.Text, Icon);
                }
            });
        }

        private void gitUiCommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
        {
            if (_currentCommitTemplates == null)
            {
                return;
            }

            foreach (var message in _currentCommitTemplates)
            {
                e.GitUICommands.RemoveCommitTemplate(message.Title);
            }

            _currentCommitTemplates = null;
        }

        private async Task<IList<CommitTemplate>> GetCommitTemplatesAsync(HttpClient client, string serviceUrl, string wiqlQuery, string stringTemplate)
        {
            try
            {
                if (!serviceUrl.EndsWith("/"))
                {
                    serviceUrl += "/";
                }

                var uri = new Uri(new Uri(serviceUrl), "_apis/wit/wiql?api-version=5.0");

                using (var request = new HttpRequestMessage(new HttpMethod("POST"), uri))
                {
                    request.Content = new StringContent("{\"query\": \"" + wiqlQuery + "\"}", Encoding.UTF8, "application/json");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.SendAsync(request).ConfigureAwait(true);
                    if (!response.IsSuccessStatusCode)
                    {
                        return new[] { new CommitTemplate($"Call error", response.ReasonPhrase) };
                    }

                    try
                    {
                        var commitTemplates = new List<CommitTemplate>();
                        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var workItems = JObject.Parse(responseContent)["workItems"];
                        if (workItems == null)
                        {
                            return commitTemplates;
                        }

                        foreach (JToken workitem in workItems)
                        {
                            commitTemplates.Add(await GetWorkItemAsync(client, workitem["id"].ToString(), workitem["url"].ToString(), stringTemplate));
                        }

                        return commitTemplates;
                    }
                    catch (Exception e)
                    {
                        return new[] { new CommitTemplate(ParsingResponseError.Text, e.Message) };
                    }
                }
            }
            catch (Exception ex)
            {
                return new[] { new CommitTemplate($"{Description} {Error}", ex.ToString()) };
            }
        }

        private async Task<CommitTemplate> GetWorkItemAsync(HttpClient client, string id, string url,
            string template)
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                var response = await client.SendAsync(request).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var workItemData = JObject.Parse(responseContent)["fields"];
                        if (_btnPreview != null)
                        {
                            _allFieldsAndValues = ExtractAllFields(workItemData);
                        }

                        return new CommitTemplate(PopulateTemplate("{id}: {System.Title}", id, workItemData), PopulateTemplate(template, id, workItemData));
                    }
                    catch (Exception e)
                    {
                        return new CommitTemplate(ParsingResponseError.Text, e.Message);
                    }
                }

                return new CommitTemplate(ApiCallError.Text, response.ReasonPhrase);
            }
        }

        private string Elide(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var indexOfNewLine = value.IndexOf("\n");
            if (indexOfNewLine != -1)
            {
                return value.Substring(0, indexOfNewLine) + "[...]";
            }

            return value.Length > 80 ? value.Substring(0, 80) + "[...]" : value;
        }

        private string ExtractAllFields(JToken fields)
        {
            List<string> allFields = new List<string>();
            foreach (var pair in (JObject)fields)
            {
                if (!pair.Value.HasValues)
                {
                    allFields.Add($"{{{pair.Key}}}: {Elide(pair.Value.ToString())}");
                }
                else
                {
                    allFields.AddRange(FlattenFieldsHierarchy(pair.Key, pair.Value));
                }
            }

            return string.Join(Environment.NewLine, allFields.OrderBy(s => s));
        }

        private IEnumerable<string> FlattenFieldsHierarchy(string key, JToken fields)
        {
            foreach (var pair in (JObject)fields)
            {
                if (!pair.Value.HasValues)
                {
                    yield return $"{{{key}|{pair.Key}}}: {Elide(pair.Value.ToString())}";
                }
                else
                {
                    foreach (string value in FlattenFieldsHierarchy(key + "|" + pair.Key, pair.Value))
                    {
                        yield return value;
                    }
                }
            }
        }

        private string PopulateTemplate(string titleTemplate, string id, JToken workitem)
        {
            return ExtractWorkItemField(titleTemplate.Replace("{id}", id), workitem);
        }

        private static bool TryGetWorkItemValue(JToken workitem, string workItemField, out string value)
        {
            if (workItemField.Contains('|'))
            {
                var fields = workItemField.Split('|');
                foreach (var field in fields)
                {
                    var foundValue = workitem[field];
                    if (foundValue == null)
                    {
                        value = null;
                        return false;
                    }

                    workitem = foundValue;
                }

                value = workitem.ToString();
                return true;
            }
            else
            {
                var foundValue = workitem[workItemField];
                value = foundValue?.ToString();
                if (workItemField == "System.Description")
                {
                    value = ExtractTextFromHtml(value);
                }

                return value != null;
            }
        }

        public static string ExtractWorkItemField(string pattern, JToken workitem)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return string.Empty;
            }

            Regex reg = new Regex("{([^}]+)}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = reg.Matches(pattern);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    var workItemField = match.Groups[1].Value;
                    if (TryGetWorkItemValue(workitem, workItemField, out var value))
                    {
                        pattern = pattern.Replace($"{{{workItemField}}}", value);
                    }
                }
            }

            return pattern;
        }

        public static string ExtractTextFromHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var htmlWithLines = html.Replace("<br>", Environment.NewLine);
            if (htmlWithLines.StartsWith("<div>", StringComparison.InvariantCultureIgnoreCase)
                && htmlWithLines.EndsWith("</div>", StringComparison.InvariantCultureIgnoreCase))
            {
                htmlWithLines = htmlWithLines.Substring(5, htmlWithLines.Length - 11);
            }

            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            string s = reg.Replace(htmlWithLines, " ");
            s = HttpUtility.HtmlDecode(s);
            return s;
        }

        private class CommitTemplate
        {
            public string Title { get; }
            public string Text { get; }

            public CommitTemplate(string title, string text)
            {
                Title = title;
                Text = text;
            }
        }
    }
}
