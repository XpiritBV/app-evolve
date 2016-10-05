using XamarinEvolve.DataObjects;
using ColoredConsole;

namespace PhotoSizeChecker.Data
{
    class AugmentedSpeaker: Speaker
    {
        public long? PhotoFileSize { get; set; }
        public long? AvatarFileSize { get; set; }

        public ColorToken AvatarFileSizeText
        {
            get
            {
                return Colorize(AvatarFileSize);
            }
        }

        private ColorToken Colorize(long? size)
        {
            if (size.HasValue)
            {
                var text = FormatSize(size.Value);
                if (size > 300000)
                {
                    if (size > 600000)
                    {
                        return text.Gray().OnRed();
                    }
                    return text.DarkYellow();
                }
                return text.DarkGreen();
            }
            return "unknown size".Red();
        }

        public ColorToken PhotoFileSizeText
        {
            get
            {
                return Colorize(PhotoFileSize);
            }
        }

        private string FormatSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            var len = size;
            while (len >= 1024 && ++order < sizes.Length)
            {
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = $"{len:0.##} {sizes[order]}";

            return result;
        }
    }
}
