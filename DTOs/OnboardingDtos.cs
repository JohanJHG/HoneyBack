namespace HoneyBack.DTOs
{
    public class OnboardingStepDto
    {
        public string Id { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool Completed { get; set; }
        public bool Locked { get; set; }
        public string? Href { get; set; }
    }

    public class OnboardingStatusDto
    {
        public bool Dismissed { get; set; }
        public int CompletedCount { get; set; }
        public int TotalSteps { get; set; }
        public int CurrentStep { get; set; }
        public int? EntornoId { get; set; }
        public List<OnboardingStepDto> Steps { get; set; } = new();
    }
}
