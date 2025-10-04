using NpgsqlTypes;

namespace CHUYENXEBACAI.Domain;

public enum UserStatus { [PgName("ACTIVE")] Active, [PgName("INACTIVE")] Inactive }
public enum AppReviewStatus { [PgName("PENDING_REVIEW")] PendingReview, [PgName("APPROVED")] Approved, [PgName("REJECTED")] Rejected }
public enum RegistrationStatus { [PgName("PENDING")] Pending, [PgName("APPROVED")] Approved, [PgName("REJECTED")] Rejected, [PgName("CANCELLED")] Cancelled }
public enum CampaignStatus { [PgName("PLANNING")] Planning, [PgName("ONGOING")] Ongoing, [PgName("DONE")] Done, [PgName("CANCELLED")] Cancelled }
public enum SessionStatus { [PgName("PLANNED")] Planned, [PgName("ONGOING")] Ongoing, [PgName("DONE")] Done }
public enum SessionShift { [PgName("MORNING")] Morning, [PgName("AFTERNOON")] Afternoon, [PgName("EVENING")] Evening }
public enum CheckinMethod { [PgName("QR")] Qr, [PgName("MANUAL")] Manual }
public enum CheckinStatus { [PgName("ON_TIME")] OnTime, [PgName("LATE")] Late, [PgName("INVALID")] Invalid }
public enum DonationGateway { [PgName("MOMO")] MoMo, [PgName("VNPAY")] VnPay, [PgName("STRIPE")] Stripe }
public enum DonationStatus { [PgName("PENDING")] Pending, [PgName("PAID")] Paid, [PgName("FAILED")] Failed, [PgName("CANCELLED")] Cancelled }
public enum FundSource { [PgName("WEBHOOK")] Webhook, [PgName("CSV")] Csv, [PgName("EMAIL")] Email, [PgName("MANUAL")] Manual }
public enum FundStatus { [PgName("PENDING")] Pending, [PgName("MATCHED")] Matched, [PgName("FLAGGED")] Flagged }
public enum BankImportSource { [PgName("CSV")] Csv, [PgName("EMAIL")] Email }
public enum ReconcileDecision { [PgName("AUTO")] Auto, [PgName("ACCEPT")] Accept, [PgName("REJECT")] Reject, [PgName("REVIEW")] Review, [PgName("UNMATCH")] Unmatch }
public enum Currency { [PgName("VND")] Vnd, [PgName("USD")] Usd, [PgName("EUR")] Eur }
public enum ChangeAction { [PgName("CREATE")] Create, [PgName("UPDATE")] Update, [PgName("DELETE")] Delete }
public enum PostStatus { [PgName("DRAFT")] Draft, [PgName("PUBLISHED")] Published }
