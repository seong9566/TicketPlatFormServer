using System.Data;
using System.Text.Json;
using Dapper;
using TicketPlatFormServer.DTO;

namespace TicketPlatFormServer.Repository.EventRepo;

/// <summary>
/// 이벤트 관련 Repository 구현체
/// </summary>
public partial class EventRepository : IEventRepository
{
    private readonly TicketContext _db;
    private readonly IDbConnection _dapper;

    public EventRepository(TicketContext db , IDbConnection dapper)
    {
        _db = db; 
        _dapper = dapper;
    }

    public async Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId)
    {
        var result = await _dapper.QueryAsync<EventListRespDto>(
            SqlGetEventsByCategoryId, 
            new { CategoryId = categoryId }
        );
        
        return result.ToList();
    }

    public async Task<EventDetailRespDto?> GetEventDetailById(int eventId)
    {
        // 이벤트 상세 정보 조회
        var eventDetail = await _dapper.QueryFirstOrDefaultAsync<EventDetailRespDto>(
            SqlGetEventDetailById,
            new { EventId = eventId }
        );

        if (eventDetail == null)
        {
            return null;
        }

        // 티켓 목록 조회 (Raw 데이터)
        var ticketRows = await _dapper.QueryAsync<dynamic>(
            SqlGetTicketsByEventId,
            new { EventId = eventId }
        );

        var tickets = new List<TicketListRespDto>();
        var seatTypeCounts = new Dictionary<string, int>(); // 좌석 타입별 개수
        bool isSoldOutImminent = false;

        foreach (var row in ticketRows)
        {
            // 좌석 타입 추출 (title에서 추출 또는 seat_features에서 추출)
            string? seatType = ExtractSeatType(row.TicketTitle, row.SeatFeatures);
            
            // 좌석 타입별 개수 집계
            if (!string.IsNullOrEmpty(seatType))
            {
                if (!seatTypeCounts.ContainsKey(seatType))
                {
                    seatTypeCounts[seatType] = 0;
                }
                seatTypeCounts[seatType]++;
            }

            // 매진 임박 체크 (remaining_quantity가 5개 이하)
            if (row.RemainingQuantity <= 5)
            {
                isSoldOutImminent = true;
            }

            // SeatFeatures JSON 파싱
            List<string> seatFeatures = new();
            if (row.SeatFeatures != null)
            {
                try
                {
                    var features = JsonSerializer.Deserialize<List<string>>(row.SeatFeatures.ToString() ?? "[]");
                    if (features != null)
                    {
                        seatFeatures = features;
                    }
                }
                catch
                {
                    // JSON 파싱 실패 시 무시
                }
            }

            tickets.Add(new TicketListRespDto
            {
                TicketId = row.TicketId,
                TicketTitle = row.TicketTitle,
                SeatInfo = row.SeatInfo,
                SeatType = seatType,
                Price = row.Price,
                OriginalPrice = row.OriginalPrice,
                SeatFeatures = seatFeatures,
                Description = row.Description,
                CreatedAt = row.CreatedAt,
                Seller = new SellerInfoDto
                {
                    UserId = row.UserId,
                    Nickname = row.Nickname,
                    ProfileImageUrl = row.ProfileImageUrl,
                    MannerTemperature = row.MannerTemperature != null ? (float?)Convert.ToDouble(row.MannerTemperature) : null
                }
            });
        }

        // 좌석 타입 필터 생성
        var seatTypeFilters = new List<SeatTypeFilterDto>();
        
        // 전체좌석 추가
        seatTypeFilters.Add(new SeatTypeFilterDto
        {
            SeatTypeName = "전체좌석",
            TicketCount = tickets.Count
        });

        // 각 좌석 타입별 필터 추가
        foreach (var kvp in seatTypeCounts.OrderBy(x => x.Key))
        {
            seatTypeFilters.Add(new SeatTypeFilterDto
            {
                SeatTypeName = kvp.Key,
                TicketCount = kvp.Value
            });
        }

        eventDetail.SeatTypeFilters = seatTypeFilters;
        eventDetail.Tickets = tickets;
        eventDetail.IsSoldOutImminent = isSoldOutImminent;

        return eventDetail;
    }

    /// <summary>
    /// 티켓 제목 또는 seat_features에서 좌석 타입 추출
    /// </summary>
    private string? ExtractSeatType(string? ticketTitle, object? seatFeatures)
    {
        if (!string.IsNullOrEmpty(ticketTitle))
        {
            // 티켓 제목에서 좌석 타입 추출 (예: "위키드 VIP석" -> "VIP석")
            if (ticketTitle.Contains("VIP"))
                return "VIP석";
            if (ticketTitle.Contains("R석") || ticketTitle.Contains(" R "))
                return "R석";
            if (ticketTitle.Contains("S석") || ticketTitle.Contains(" S "))
                return "S석";
            if (ticketTitle.Contains("A석") || ticketTitle.Contains(" A "))
                return "A석";
        }

        // seat_features JSON에서 추출
        if (seatFeatures != null)
        {
            try
            {
                var features = JsonSerializer.Deserialize<List<string>>(seatFeatures.ToString() ?? "[]");
                if (features != null)
                {
                    // VIP, R, S, A 등 좌석 타입 키워드 찾기
                    var seatTypeKeywords = new[] { "VIP", "R", "S", "A" };
                    foreach (var keyword in seatTypeKeywords)
                    {
                        if (features.Any(f => f.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                        {
                            return keyword == "VIP" ? "VIP석" : $"{keyword}석";
                        }
                    }
                }
            }
            catch
            {
                // JSON 파싱 실패 시 무시
            }
        }

        return null;
    }
}

