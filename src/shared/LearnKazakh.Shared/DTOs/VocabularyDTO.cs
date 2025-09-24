using LearnKazakh.Shared.Enums;

namespace LearnKazakh.Shared.DTOs;

public class VocabularyDto
{
    public Guid Id { get; set; }
    public string WordKazakh { get; set; } = string.Empty;
    public string WordAzerbaijani { get; set; } = string.Empty;
    public string? Pronounciation { get; set; }
    public string? AudioUrl { get; set; }
    public string Type { get; set; } = string.Empty;
    public List<VocabularyExampleDto> Examples { get; set; } = [];
}

public class CreateVocabularyDto
{
    public string WordKazakh { get; set; } = string.Empty;
    public string WordAzerbaijani { get; set; } = string.Empty;
    public string? Pronounciation { get; set; }
    public string? AudioUrl { get; set; }
    public VocabularyType Type { get; set; } = VocabularyType.All;
}

public class UpdateVocabularyDto
{
    public Guid Id { get; set; }
    public string WordKazakh { get; set; } = string.Empty;
    public string WordAzerbaijani { get; set; } = string.Empty;
    public string? Pronounciation { get; set; }
    public string? AudioUrl { get; set; }
    public VocabularyType Type { get; set; } = VocabularyType.All;
}

public class DetailedVocabularyDto
{
    public Guid Id { get; set; }
    public string WordKazakh { get; set; } = string.Empty;
    public string WordAzerbaijani { get; set; } = string.Empty;
    public string? Pronounciation { get; set; }
    public string? AudioUrl { get; set; }
    public VocabularyType Type { get; set; } = VocabularyType.All;
    public List<VocabularyExampleDto> Examples { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class VocabularyStatsDto
{
    public int TotalWords { get; set; }
    public Dictionary<string, int> Categories { get; set; } = [];
}