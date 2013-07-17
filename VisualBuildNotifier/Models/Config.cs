namespace VisualBuildNotifier.Models
{
    public class Config {
        public string Project { get; set; }
        public string Server { get; set; }
        public string Build { get; set; }

        public bool Blink1Enabled { get; set; }

        public bool IsComplete {
            get {
                return
                    !string.IsNullOrWhiteSpace(Server)
                    && !string.IsNullOrWhiteSpace(Project)
                    && !string.IsNullOrWhiteSpace(Build);
            }
        }
    }
}
