using System;

namespace KimmyEsthi.ConsentForm;

public class SkincareHistoryQuestionnaire
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public required bool EverReceivedFacial { get; set; }
    public DateTime? LastFacialDate { get; set; }
    public required bool Retinol { get; set; }
    public required bool ChemPeel { get; set; }
    public DateTime? LastChemPeelDate { get; set; }
    public required bool HairRemoval { get; set; }
    public required bool MedicalConditions { get; set; }
    public required bool Allergies { get; set; }
    public required bool Botox { get; set; }
    public required bool NegativeReaction { get; set; }
    public required string SkinType { get; set; }
}
