using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitUIPluginInterfaces;

namespace GitCommands.Gpg
{
    public enum CommitStatus
    {
        NoSignature = 0,
        GoodSignature = 1,
        SignatureError = 2,
        MissingPublicKey = 3,
    }

    public enum TagStatus
    {
        NoTag = 0,
        OneGood = 1,
        OneBad = 2,
        Many = 3,
        NoPubKey = 4,
        TagNotSigned = 5
    }

    public interface IGitGpgController
    {
        /// <summary>
        /// Obtain the commit signature status on current revision.
        /// </summary>
        /// <returns>Enum value that indicate the gpg status for current git revision.</returns>
        Task<CommitStatus> GetRevisionCommitSignatureStatusAsync(GitRevision revision);

        /// <summary>
        /// Obtain the commit verification message, coming from --pretty="format:%GG"
        /// </summary>
        /// <returns>Full string coming from GPG analysis on current revision.</returns>
        string GetCommitVerificationMessage(GitRevision revision);

        /// <summary>
        /// Obtain the tag status on current revision.
        /// </summary>
        /// <returns>Enum value that indicate if current git revision has one tag with good signature, one tag with bad signature or more than one tag.</returns>
        Task<TagStatus> GetRevisionTagSignatureStatusAsync(GitRevision revision);

        /// <summary>
        /// Obtain the tag verification message for all the tag in current git revision
        /// </summary>
        /// <returns>Full concatenated string coming from GPG analysis on all tags on current git revision.</returns>
        string GetTagVerifyMessage(GitRevision revision);
    }

    public class GitGpgController : IGitGpgController
    {
        private readonly Func<IGitModule> _getModule;

        /* Commit GPG status */
        private const string GoodSign = "G";
        private const string BadSign = "B";
        private const string UnkSignValidity = "U";
        private const string ExpiredSign = "X";
        private const string ExpiredSignKey = "Y";
        private const string RevokedKey = "R";
        private const string MissingPubKey = "E";
        private const string NoSign = "N";

        /* Tag GPG status */
        private const string GoodSignature = "GOODSIG";
        private const string ValidTagSign = "VALIDSIG";
        private const string NoTagPubKey = "NO_PUBKEY";
        private const string NoSignatureFound = "error: no signature found";

        private static readonly Regex ValidSignatureTagRegex = new Regex(ValidTagSign, RegexOptions.Compiled);
        private static readonly Regex GoodSignatureTagRegex = new Regex(GoodSignature, RegexOptions.Compiled);
        private static readonly Regex NoPubKeyTagRegex = new Regex(NoTagPubKey, RegexOptions.Compiled);
        private static readonly Regex NoSignatureFoundTagRegex = new Regex(NoSignatureFound, RegexOptions.Compiled);

        /// <summary>
        /// Obtain the tag verification message for all the tag in current git revision
        /// </summary>
        /// <returns>Full concatenated string coming from GPG analysis on all tags on current git revision.</returns>
        public GitGpgController(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        /// <summary>
        /// Obtain the commit signature status on current revision.
        /// </summary>
        /// <returns>Enum value that indicate the gpg status for current git revision.</returns>
        public async Task<CommitStatus> GetRevisionCommitSignatureStatusAsync(GitRevision revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            var module = GetModule();

            return await Task.Run(() =>
            {
                CommitStatus cmtStatus;
                var args = new GitArgumentBuilder("log")
                {
                    "--pretty=\"format:%G?\"",
                    "-1",
                    revision.Guid
                };
                string gpg = module.GitExecutable.GetOutput(args);

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
                switch (gpg)
                {
                    case GoodSign:         // "G" for a good (valid) signature
                        cmtStatus = CommitStatus.GoodSignature;
                        break;
                    case BadSign:          // "B" for a bad signature
                    case UnkSignValidity:  // "U" for a good signature with unknown validity
                    case ExpiredSign:      // "X" for a good signature that has expired
                    case ExpiredSignKey:   // "Y" for a good signature made by an expired key
                    case RevokedKey:       // "R" for a good signature made by a revoked key
                        cmtStatus = CommitStatus.SignatureError;
                        break;
                    case MissingPubKey:    // "E" if the signature cannot be checked (e.g.missing key)
                        cmtStatus = CommitStatus.MissingPublicKey;
                        break;
                    case NoSign:           // "N" for no signature
                    default:
                        cmtStatus = CommitStatus.NoSignature;
                        break;
                }
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row

                return cmtStatus;
            });
        }

        /// <summary>
        /// Obtain the tag status on current revision.
        /// </summary>
        /// <returns>Enum value that indicate if current git revision has one tag with good signature, one tag with bad signature or more than one tag.</returns>
        public async Task<TagStatus> GetRevisionTagSignatureStatusAsync(GitRevision revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            /* No Tag present, exit */
            var usefulTagRefs = revision.Refs.Where(x => x.IsTag && x.IsDereference).ToList();

            if (usefulTagRefs.Count == 0)
            {
                return TagStatus.NoTag;
            }

            return await Task.Run(() =>
            {
                /* More than one tag on the revision */
                if (usefulTagRefs.Count > 1)
                {
                    return TagStatus.Many;
                }

                /* Raw message to be checked */
                string rawGpgMessage = GetTagVerificationMessage(usefulTagRefs[0], true);

                /* Look for icon to be shown */
                if (GoodSignatureTagRegex.IsMatch(rawGpgMessage) && ValidSignatureTagRegex.IsMatch(rawGpgMessage))
                {
                    /* It's only one good tag */
                    return TagStatus.OneGood;
                }

                if (NoSignatureFoundTagRegex.IsMatch(rawGpgMessage))
                {
                    /* One tag, but not signed */
                    return TagStatus.TagNotSigned;
                }

                if (NoPubKeyTagRegex.IsMatch(rawGpgMessage))
                {
                    /* One tag, signed, but user has not the public key */
                    return TagStatus.NoPubKey;
                }

                /* One tag, signed, any other error */
                return TagStatus.OneBad;
            });
        }

        /// <summary>
        /// Obtain the commit verification message, coming from --pretty="format:%GG"
        /// </summary>
        /// <returns>Full string coming from GPG analysis on current revision.</returns>
        public string GetCommitVerificationMessage(GitRevision revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            var module = GetModule();
            var args = new GitArgumentBuilder("log")
            {
                "--pretty=\"format:%GG\"",
                "-1",
                revision.Guid
            };
            return module.GitExecutable.GetOutput(args);
        }

        /// <summary>
        /// Obtain the tag verification message for all the tag on the revision
        /// </summary>
        /// <returns>Full string coming from GPG analysis on current revision.</returns>
        public string GetTagVerifyMessage(GitRevision revision)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            var usefulTagRefs = revision.Refs.Where(x => x.IsTag && x.IsDereference).ToList();
            return EvaluateTagVerifyMessage(usefulTagRefs);
        }

        private string GetTagVerificationMessage(IGitRef tagRef, bool raw = true)
        {
            string tagName = tagRef.LocalName;
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return null;
            }

            var module = GetModule();
            var args = new GitArgumentBuilder("verify-tag")
            {
                { raw, "--raw" },
                tagName
            };
            return module.GitExecutable.GetOutput(args);
        }

        private string EvaluateTagVerifyMessage(IReadOnlyList<IGitRef> usefulTagRefs)
        {
            if (usefulTagRefs.Count == 0)
            {
                return "";
            }

            if (usefulTagRefs.Count == 1)
            {
                return GetTagVerificationMessage(usefulTagRefs[0], false);
            }

            /* When _usefulTagRefs.Count > 1 */
            string tagVerifyMessage = "";

            /* Only to populate TagVerifyMessage */
            foreach (var tagRef in usefulTagRefs)
            {
                /* String printed in dialog box */
                tagVerifyMessage = $"{tagVerifyMessage}{tagRef.LocalName}\r\n{GetTagVerificationMessage(tagRef, false)}\r\n\r\n";
            }

            return tagVerifyMessage;
        }

        private IGitModule GetModule()
        {
            var module = _getModule();
            if (module == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return module;
        }
    }
}
