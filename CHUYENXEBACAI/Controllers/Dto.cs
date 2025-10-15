using CHUYENXEBACAI.Domain;
using System.ComponentModel.DataAnnotations;

namespace CHUYENXEBACAI.Controllers;

public record Pagination(int Page = 1, int PageSize = 20);
public record CreateCampaignDto(string Title, string? Description, string? Location, DateOnly? StartDate, DateOnly? EndDate, decimal? GoalAmount);
public record UpdateCampaignStatusDto(CHUYENXEBACAI.Domain.CampaignStatus Status);
public record CreateSessionDto(Guid CampaignId, string? Title, DateOnly SessionDate, CHUYENXEBACAI.Domain.SessionShift Shift, int? Quota, CHUYENXEBACAI.Domain.SessionStatus Status, string? PlaceName, double? Lat, double? Lng, int? GeoRadiusM);
public record RegisterDto(Guid UserId, Guid CampaignId, Guid? SessionId);
public record ReviewRegistrationDto(CHUYENXEBACAI.Domain.RegistrationStatus Status, string? RejectReason);

// Identity 
public record CreateUserDto(
    [EmailAddress] string Email,
    string PasswordHash,           
    string FullName,
    string? Phone
);
public record AssignRoleDto(Guid UserId, string RoleCode);

// Volunteers
public record SubmitVolunteerAppDto(Guid UserId, string? Skills, string? Availability);
public record ReviewVolunteerAppDto(AppReviewStatus Status, string? RejectReason);

// Check-ins / Media
public record CreateCheckinDto(Guid SessionId, Guid UserId, CheckinMethod Method, double? Lat, double? Lng, string? EvidenceNote);
public record CreateMediaDto(Guid CampaignId, Guid CheckinId, string Url, string? PublicId, string? ThumbUrl, string? Format);

// Finance
public record CreateExpenseDto(Guid CampaignId, Guid? SessionId, string? Category, string? Description, decimal Amount, Currency Currency, string? PaymentMethod, Guid? PayerId, string? ReceiptUrl, string? Note);
public record UpsertDonationDto(Guid CampaignId, string? DonorName, string? DonorEmail, decimal Amount, Currency Currency, bool WishToShowName, string? Message, DonationGateway? Gateway, string? OrderCode, DonationStatus? Status, DateTime? PaidAt);
public record UpsertFundTxDto(Guid CampaignId, FundSource Source, string RefId, decimal Amount, DateTime OccurredAt, Guid? DonationId);
public record DecideMatchDto(ReconcileDecision Decision, string? Note);
public record UpsertPostDto(Guid CampaignId, string Title, string? ContentMd, string? CoverUrl, PostStatus Status);
public record UpsertFaqDto(string Question, string? AnswerMd, string[]? Tags, int? OrderNo);
public record SubscribeDto([EmailAddress] string Email, bool Consent);