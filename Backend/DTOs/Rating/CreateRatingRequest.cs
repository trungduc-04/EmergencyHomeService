namespace EmergencyHomeService.API.DTOs.Rating;

public class CreateRatingRequest
{
    public int Score { get; set; }

    public string? Comment { get; set; }
}