namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

internal enum MultiRepositorySyncLabelKind
{
    Neutral,
    Synchronized,
    Ahead,
    Behind,
    Diverged,
    Error
}

internal sealed record MultiRepositorySyncLabel(string Text, MultiRepositorySyncLabelKind Kind);

internal static class MultiRepositoryStatusPresentation
{
    public static IReadOnlyList<MultiRepositorySyncLabel> GetSynchronizationLabels(MultiRepositoryStatus? status)
    {
        if (status is null)
        {
            return [new("等待检查", MultiRepositorySyncLabelKind.Neutral)];
        }

        if (status.IsDetached)
        {
            return [new("分离 HEAD", MultiRepositorySyncLabelKind.Neutral)];
        }

        if (string.IsNullOrWhiteSpace(status.Upstream))
        {
            return [new("未设置上游", MultiRepositorySyncLabelKind.Neutral)];
        }

        int ahead = status.Ahead ?? 0;
        int behind = status.Behind ?? 0;
        if (ahead == 0 && behind == 0)
        {
            return [new("已同步", MultiRepositorySyncLabelKind.Synchronized)];
        }

        List<MultiRepositorySyncLabel> labels = [];
        if (ahead != 0 && behind != 0)
        {
            labels.Add(new("已分叉", MultiRepositorySyncLabelKind.Diverged));
        }

        if (ahead != 0)
        {
            labels.Add(new($"领先 {ahead}", MultiRepositorySyncLabelKind.Ahead));
        }

        if (behind != 0)
        {
            labels.Add(new($"落后 {behind}", MultiRepositorySyncLabelKind.Behind));
        }

        return labels;
    }

    public static string FormatWorkingTree(MultiRepositoryStatus? status)
    {
        if (status is null)
        {
            return "等待检查";
        }

        if (status.IsBare)
        {
            return "裸仓库";
        }

        if (!status.HasWorkingTreeChanges)
        {
            return "干净";
        }

        List<string> parts = [];
        if (status.StagedCount != 0)
        {
            parts.Add($"已暂存 {status.StagedCount}");
        }

        if (status.ModifiedCount != 0)
        {
            parts.Add($"已修改 {status.ModifiedCount}");
        }

        if (status.UntrackedCount != 0)
        {
            parts.Add($"未跟踪 {status.UntrackedCount}");
        }

        return string.Join(" · ", parts);
    }

    public static string FormatFetchTimestamp(DateTimeOffset? timestamp, DateTimeOffset now)
        => timestamp is null || timestamp == default
            ? "从未"
            : $"{FormatRelativeTime(timestamp.Value, now)}（{timestamp.Value.ToLocalTime():yyyy-MM-dd HH:mm}）";

    public static string FormatCheckedTimestamp(DateTimeOffset? timestamp)
        => timestamp is null || timestamp == default
            ? "从未"
            : timestamp.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    internal static string FormatRelativeTime(DateTimeOffset timestamp, DateTimeOffset now)
    {
        TimeSpan elapsed = now - timestamp;
        if (elapsed < TimeSpan.FromMinutes(1))
        {
            return "刚刚";
        }

        if (elapsed < TimeSpan.FromHours(1))
        {
            return $"{(int)elapsed.TotalMinutes} 分钟前";
        }

        if (elapsed < TimeSpan.FromDays(1))
        {
            return $"{(int)elapsed.TotalHours} 小时前";
        }

        if (elapsed < TimeSpan.FromDays(30))
        {
            return $"{(int)elapsed.TotalDays} 天前";
        }

        if (elapsed < TimeSpan.FromDays(365))
        {
            return $"{Math.Max(1, (int)(elapsed.TotalDays / 30))} 个月前";
        }

        return $"{Math.Max(1, (int)(elapsed.TotalDays / 365))} 年前";
    }
}
