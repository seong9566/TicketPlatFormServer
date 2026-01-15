namespace TicketPlatFormServer.Repository.Home;

/// <summary>
/// HomeRepository SQL 쿼리 모음 (Static Class 패턴)
/// </summary>
internal static class HomeQueries
{
    /// <summary>
    /// 배너 목록 조회 SQL (임시: 인기 이벤트 기반)
    /// </summary>
    internal const string GetBanners = @"
        SELECT
            e.id AS BannerId,
            e.title AS Title,
            e.poster_image_url AS ImageUrl,
            CONCAT('/events/', e.id) AS LinkUrl,
            e.sort_order AS DisplayOrder
        FROM events e
        WHERE e.is_active = 1
        ORDER BY e.sort_order ASC
        LIMIT 5";

    /// <summary>
    /// 카테고리 목록 조회 SQL
    /// </summary>
    internal const string GetCategories = @"
        SELECT
            id AS CategoryId,
            name_ko AS CategoryName,
            code AS IconName,
            sort_order AS DisplayOrder
        FROM ticket_category
        WHERE is_active = 1
        ORDER BY sort_order ASC";

    /// <summary>
    /// 인기 공연 목록 조회 SQL (판매량 50% + 찜개수 30% + 최신도 20% 복합 점수)
    /// </summary>
    internal const string GetPopularEvents = @"
        SELECT
            e.id AS EventId,
            e.title AS EventTitle,
            e.description AS EventDescription,
            DATE_FORMAT(e.start_at, '%Y.%m.%d') AS EventDate,
            e.venue_name AS Venue,
            MIN(t.price) AS MinTicketPrice,
            MIN(t.original_price) AS OriginalMinTicketPrice,
            ROUND((MIN(t.original_price) - MIN(t.price)) / MIN(t.original_price) * 100) AS TicketDiscountRate,
            e.poster_image_url AS PosterImageUrl,
            COUNT(t.id) AS AvailableTicketCount,
            e.category_id AS CategoryId
        FROM events e
        INNER JOIN tickets t ON e.id = t.event_id
        LEFT JOIN (
            SELECT target_id, COUNT(*) as favorite_count
            FROM user_favorites
            WHERE favorite_type_id = 1
            GROUP BY target_id
        ) fav ON e.id = fav.target_id
        WHERE e.is_active = 1
            AND t.status_id = 1
            AND t.deleted_at IS NULL
        GROUP BY e.id, e.title, e.description, e.start_at, e.venue_name, e.poster_image_url, e.category_id
        ORDER BY
            -- 인기도 복합 점수 = 판매량(50%) + 찜개수(30%) + 최신도(20%)
            (
                -- 판매량: 판매된 티켓 수 (quantity - remaining_quantity)
                (SUM(t.quantity) - SUM(t.remaining_quantity)) * 0.5 +
                -- 찜 개수: 이벤트를 찜한 사용자 수
                COALESCE(fav.favorite_count, 0) * 0.3 +
                -- 최신도: 최근 30일 이내 생성된 이벤트에 가산점 (0~30점)
                GREATEST(0, 30 - DATEDIFF(NOW(), e.created_at)) * 0.2
            ) DESC
        LIMIT @Limit";

    /// <summary>
    /// 추천 이벤트 목록 조회 SQL (로그인 사용자용 - 하이브리드 추천)
    /// 협업 필터링(40%) + 카테고리 매칭(30%) + 인기도(20%) + 다양성(10%)
    /// </summary>
    internal const string GetRecommendedEventsForUser = @"
        WITH
        -- 1. 협업 필터링: 나와 비슷한 취향의 사용자들이 찜한 이벤트
        CollaborativeScores AS (
            SELECT
                uf.target_id AS event_id,
                COUNT(DISTINCT uf.user_id) AS similar_user_count
            FROM user_favorites uf
            WHERE uf.favorite_type_id = 1
                AND uf.user_id IN (
                    -- 나와 찜 목록이 겹치는 사용자들 (유사 사용자)
                    SELECT DISTINCT uf2.user_id
                    FROM user_favorites uf2
                    WHERE uf2.target_id IN (
                        SELECT target_id
                        FROM user_favorites
                        WHERE user_id = @UserId AND favorite_type_id = 1
                    )
                    AND uf2.user_id != @UserId
                    AND uf2.favorite_type_id = 1
                )
                AND uf.target_id NOT IN (
                    -- 내가 이미 찜한 이벤트 제외
                    SELECT target_id
                    FROM user_favorites
                    WHERE user_id = @UserId AND favorite_type_id = 1
                )
            GROUP BY uf.target_id
        ),
        -- 2. 사용자 카테고리 선호도 분석
        UserCategoryPreferences AS (
            SELECT DISTINCT e.category_id
            FROM user_favorites uf
            JOIN events e ON uf.target_id = e.id
            WHERE uf.user_id = @UserId
                AND uf.favorite_type_id = 1
        ),
        -- 3. 이벤트 인기도 점수 (판매량 + 찜 개수)
        EventPopularity AS (
            SELECT
                e.id AS event_id,
                -- 판매량 + 찜 개수를 정규화 (0~100 범위)
                (
                    (SUM(t.quantity) - SUM(t.remaining_quantity)) * 0.5 +
                    COALESCE(fav_count.cnt, 0) * 0.5
                ) AS popularity_score
            FROM events e
            LEFT JOIN tickets t ON e.id = t.event_id
                AND t.deleted_at IS NULL
                AND t.status_id = 1
            LEFT JOIN (
                SELECT target_id, COUNT(*) as cnt
                FROM user_favorites
                WHERE favorite_type_id = 1
                GROUP BY target_id
            ) fav_count ON e.id = fav_count.target_id
            GROUP BY e.id, fav_count.cnt
        )
        -- 최종 추천 쿼리
        SELECT
            e.id AS EventId,
            e.title AS EventTitle,
            e.description AS EventDescription,
            DATE_FORMAT(e.start_at, '%Y.%m.%d') AS EventDate,
            e.venue_name AS Venue,
            MIN(t.price) AS MinTicketPrice,
            MIN(t.original_price) AS OriginalMinTicketPrice,
            ROUND((MIN(t.original_price) - MIN(t.price)) / MIN(t.original_price) * 100) AS TicketDiscountRate,
            e.poster_image_url AS PosterImageUrl,
            COUNT(t.id) AS AvailableTicketCount,
            e.category_id AS CategoryId,
            TRUE AS IsWishedByMe,
            -- 하이브리드 추천 점수 계산
            (
                -- 협업 필터링 점수 (40%): 유사 사용자들이 많이 찜한 이벤트
                COALESCE(cs.similar_user_count, 0) * 0.4 +
                -- 카테고리 매칭 점수 (30%): 내가 찜한 카테고리와 일치
                CASE
                    WHEN e.category_id IN (SELECT category_id FROM UserCategoryPreferences)
                    THEN 30
                    ELSE 0
                END * 0.3 +
                -- 인기도 점수 (20%): 실제 판매량과 찜 개수
                COALESCE(ep.popularity_score, 0) * 0.2 +
                -- 다양성 점수 (10%): 새로운 카테고리 발견 보너스
                CASE
                    WHEN e.category_id NOT IN (SELECT category_id FROM UserCategoryPreferences)
                    THEN 10
                    ELSE 0
                END * 0.1
            ) AS RecommendationScore
        FROM events e
        INNER JOIN tickets t ON e.id = t.event_id
        LEFT JOIN CollaborativeScores cs ON e.id = cs.event_id
        LEFT JOIN EventPopularity ep ON e.id = ep.event_id
        WHERE e.is_active = 1
            AND t.status_id = 1
            AND t.deleted_at IS NULL
            AND e.id NOT IN (
                SELECT target_id FROM user_favorites
                WHERE user_id = @UserId AND favorite_type_id = 1
            )
        GROUP BY e.id, e.title, e.description, e.start_at, e.venue_name,
                 e.poster_image_url, e.category_id, cs.similar_user_count, ep.popularity_score
        HAVING AvailableTicketCount > 0
        ORDER BY RecommendationScore DESC
        LIMIT @Limit";

    /// <summary>
    /// 추천 이벤트 목록 조회 SQL (비로그인 사용자용 - 인기도 + 카테고리 다양성)
    /// 인기도(60%) + 할인율(20%) + 최신도(20%) + 카테고리 다양성 확보
    /// </summary>
    internal const string GetRecommendedEventsForGuest = @"
        WITH
        -- 카테고리별 대표 이벤트 선정 (다양성 확보)
        CategoryTopEvents AS (
            SELECT
                e.id AS event_id,
                e.category_id,
                -- 카테고리 내 인기도 순위
                ROW_NUMBER() OVER (
                    PARTITION BY e.category_id
                    ORDER BY
                        (SUM(t.quantity) - SUM(t.remaining_quantity)) * 0.5 +
                        COALESCE(fav.favorite_count, 0) * 0.5
                    DESC
                ) AS category_rank
            FROM events e
            INNER JOIN tickets t ON e.id = t.event_id
                AND t.deleted_at IS NULL
                AND t.status_id = 1
            LEFT JOIN (
                SELECT target_id, COUNT(*) as favorite_count
                FROM user_favorites
                WHERE favorite_type_id = 1
                GROUP BY target_id
            ) fav ON e.id = fav.target_id
            WHERE e.is_active = 1
            GROUP BY e.id, e.category_id, fav.favorite_count
        )
        -- 최종 추천 쿼리
        SELECT
            e.id AS EventId,
            e.title AS EventTitle,
            e.description AS EventDescription,
            DATE_FORMAT(e.start_at, '%Y.%m.%d') AS EventDate,
            e.venue_name AS Venue,
            MIN(t.price) AS MinTicketPrice,
            MIN(t.original_price) AS OriginalMinTicketPrice,
            ROUND((MIN(t.original_price) - MIN(t.price)) / MIN(t.original_price) * 100) AS TicketDiscountRate,
            e.poster_image_url AS PosterImageUrl,
            COUNT(t.id) AS AvailableTicketCount,
            e.category_id AS CategoryId,
            FALSE AS IsWishedByMe,
            -- 추천 점수 계산
            (
                -- 인기도 점수 (60%): 판매량 + 찜 개수
                (
                    (SUM(t.quantity) - SUM(t.remaining_quantity)) * 0.5 +
                    COALESCE(fav.favorite_count, 0) * 0.5
                ) * 0.6 +
                -- 할인율 점수 (20%): 할인율이 높을수록 매력적
                AVG(ROUND((t.original_price - t.price) / t.original_price * 100)) * 0.2 +
                -- 최신도 점수 (20%): 최근 30일 이내 가산점
                GREATEST(0, 30 - DATEDIFF(NOW(), e.created_at)) * 0.2 +
                -- 카테고리 다양성 보너스: 각 카테고리의 상위 이벤트에 가산점
                CASE WHEN cte.category_rank <= 2 THEN 10 ELSE 0 END
            ) AS RecommendationScore
        FROM events e
        INNER JOIN tickets t ON e.id = t.event_id
        LEFT JOIN (
            SELECT target_id, COUNT(*) as favorite_count
            FROM user_favorites
            WHERE favorite_type_id = 1
            GROUP BY target_id
        ) fav ON e.id = fav.target_id
        LEFT JOIN CategoryTopEvents cte ON e.id = cte.event_id
        WHERE e.is_active = 1
            AND t.status_id = 1
            AND t.deleted_at IS NULL
        GROUP BY e.id, e.title, e.description, e.start_at, e.venue_name,
                 e.poster_image_url, e.category_id, fav.favorite_count, cte.category_rank
        HAVING AvailableTicketCount > 0
        ORDER BY RecommendationScore DESC
        LIMIT @Limit";
}
