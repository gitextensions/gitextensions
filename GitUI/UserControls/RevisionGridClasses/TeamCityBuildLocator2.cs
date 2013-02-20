using System;
using System.Collections.Generic;
using TeamCitySharp.Locators;

namespace GitUI.UserControls.RevisionGridClasses
{
    public class TeamCityBuildLocator2 : BuildLocator
    {
        public new long? Id { get; private set; }

        public new string Number { get; private set; }

        public new string[] Tags { get; private set; }

        public new BuildTypeLocator BuildType { get; private set; }

        public new UserLocator User { get; private set; }

        public new string AgentName { get; private set; }

        public new BuildStatus? Status { get; private set; }

        public new TeamCityBuildLocator2 SinceBuild { get; private set; }

        public new bool? Personal { get; private set; }

        public new bool? Canceled { get; private set; }

        public new bool? Running { get; private set; }

        public new bool? Pinned { get; private set; }

        public new int? MaxResults { get; private set; }

        public new int? StartIndex { get; private set; }

        public new DateTime? SinceDate { get; private set; }

        public string DefaultBranch { get; private set; }

        public static TeamCityBuildLocator2 WithId(long id)
        {
            return new TeamCityBuildLocator2()
                       {
                           Id = new long?(id)
                       };
        }

        public static TeamCityBuildLocator2 WithNumber(string number)
        {
            return new TeamCityBuildLocator2()
                       {
                           Number = number
                       };
        }

        public static TeamCityBuildLocator2 RunningBuilds()
        {
            return new TeamCityBuildLocator2()
                       {
                           Running = new bool?(true)
                       };
        }

        public static TeamCityBuildLocator2 WithDimensions(BuildTypeLocator buildType = null, UserLocator user = null,
                                                   string agentName = null, BuildStatus? status = null,
                                                   bool? personal = null, bool? canceled = null, bool? running = null,
                                                   bool? pinned = null, int? maxResults = null, int? startIndex = null,
                                                   TeamCityBuildLocator2 sinceBuild = null, DateTime? sinceDate = null,
                                                   string[] tags = null, string defaultBranch = null)
        {
            return new TeamCityBuildLocator2()
                       {
                           BuildType = buildType,
                           User = user,
                           AgentName = agentName,
                           Status = status,
                           Personal = personal,
                           Canceled = canceled,
                           Running = running,
                           Pinned = pinned,
                           MaxResults = maxResults,
                           StartIndex = startIndex,
                           SinceBuild = sinceBuild,
                           SinceDate = sinceDate,
                           Tags = tags,
                           DefaultBranch = defaultBranch
                       };
        }

        public override string ToString()
        {
            if (this.Id.HasValue)
                return "id:" + (object) this.Id;
            if (this.Number != null)
                return "number:" + this.Number;
            var list = new List<string>();
            if (this.BuildType != null)
                list.Add("buildType:(" + (object) this.BuildType + ")");
            if (this.User != null)
                list.Add("user:(" + (object) this.User + ")");
            if (this.Tags != null)
                list.Add("tags:(" + string.Join(",", this.Tags) + ")");
            if (this.SinceBuild != null)
                list.Add("sinceBuild:(" + (object) this.SinceBuild + ")");
            if (!string.IsNullOrEmpty(this.AgentName))
                list.Add("agentName:" + this.AgentName);
            if (this.Status.HasValue)
                list.Add("status:" + ((object) this.Status.Value).ToString());
            if (this.Personal.HasValue)
                list.Add("personal:" + this.Personal.Value.ToString());
            if (this.Canceled.HasValue)
                list.Add("canceled:" + this.Canceled.Value.ToString());
            if (this.Running.HasValue)
                list.Add("running:" + this.Running.Value.ToString());
            if (this.Pinned.HasValue)
                list.Add("pinned:" + this.Pinned.Value.ToString());
            if (this.MaxResults.HasValue)
                list.Add("count:" + this.MaxResults.Value.ToString());
            if (this.StartIndex.HasValue)
                list.Add("start:" + this.StartIndex.Value.ToString());
            if (this.SinceDate.HasValue)
                list.Add(string.Format("sinceDate:{0}", this.SinceDate.Value.ToString("yyyyMMdd'T'HHmmsszzzz").Replace(":", "").Replace("+", "-")));
            if (!string.IsNullOrEmpty(this.DefaultBranch))
                list.Add(string.Format("branch:(default:{0})", this.DefaultBranch));
            return string.Join(",", list.ToArray());
        }
    }
}