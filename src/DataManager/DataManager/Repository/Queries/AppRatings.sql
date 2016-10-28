select     DeviceOS
,          NumberOfVotes = COUNT(1)
,          Score = CONVERT(DECIMAL(3,2), AVG(CONVERT(DECIMAL, Question5)))
,		   [OneStar] = dbo.fn_AppRatingPerPlatform(1,DeviceOS)
,		   [TwoStar] = dbo.fn_AppRatingPerPlatform(2,DeviceOS)
,		   [ThreeStar] = dbo.fn_AppRatingPerPlatform(3,DeviceOS)
,		   [FourStar] = dbo.fn_AppRatingPerPlatform(4,DeviceOS)
,		   [FiveStar] = dbo.fn_AppRatingPerPlatform(5,DeviceOS)
from       dbo.ConferenceFeedbacks f
where      (f.Deleted != 1 or f.Deleted is null)
group by   DeviceOS