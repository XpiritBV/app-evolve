select     s.Id
,		   s.Title
,          COUNT(1) as [NumberOfVotes]
,          CONVERT(DECIMAL(3,2), AVG(CONVERT(DECIMAL(3,2), f.SessionRating))) as [AverageRating]
,          [OneStar] = dbo.fn_NumberOfRatings(s.Id, 1)
,          [TwoStar] = dbo.fn_NumberOfRatings(s.Id, 2)
,          [ThreeStar] = dbo.fn_NumberOfRatings(s.Id, 3)
,          [FourStar] = dbo.fn_NumberOfRatings(s.Id, 4)
,          [FiveStar] = dbo.fn_NumberOfRatings(s.Id, 5)
from       dbo.Sessions s
inner join dbo.Feedbacks f
        on f.SessionId = s.Id
where      (f.Deleted != 1 or f.Deleted is null)
  and      (s.Deleted != 1 or s.Deleted is null)
group by   s.Id, s.Title
order by   4 desc, 3 desc