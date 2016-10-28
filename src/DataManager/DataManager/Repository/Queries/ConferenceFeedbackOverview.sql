select     Q = 1
,		   Score = CONVERT(DECIMAL(3,2), AVG(CONVERT(DECIMAL, Question1)))
,		   [OneStar] = dbo.fn_NumberOfConferenceFeedBacks(1,1)
,		   [TwoStar] = dbo.fn_NumberOfConferenceFeedBacks(1,2)
,		   [ThreeStar] = dbo.fn_NumberOfConferenceFeedBacks(1,3)
,		   [FourStar] = dbo.fn_NumberOfConferenceFeedBacks(1,4)
,		   [FiveStar] = dbo.fn_NumberOfConferenceFeedBacks(1,5)
from       dbo.ConferenceFeedbacks f
where      (f.Deleted != 1 or f.Deleted is null)
union
select     Q = 2
,          Score = CONVERT(DECIMAL(3,2), AVG(CONVERT(DECIMAL, Question2)))
,		   [OneStar] = dbo.fn_NumberOfConferenceFeedBacks(2,1)
,		   [TwoStar] = dbo.fn_NumberOfConferenceFeedBacks(2,2)
,		   [ThreeStar] = dbo.fn_NumberOfConferenceFeedBacks(2,3)
,		   [FourStar] = dbo.fn_NumberOfConferenceFeedBacks(2,4)
,		   [FiveStar] = dbo.fn_NumberOfConferenceFeedBacks(2,5)
from       dbo.ConferenceFeedbacks f
where      (f.Deleted != 1 or f.Deleted is null)
union
select     Q = 3
,          Score = CONVERT(DECIMAL(3,2), AVG(CONVERT(DECIMAL, Question3)))
,		   [OneStar] = dbo.fn_NumberOfConferenceFeedBacks(3,1)
,		   [TwoStar] = dbo.fn_NumberOfConferenceFeedBacks(3,2)
,		   [ThreeStar] = dbo.fn_NumberOfConferenceFeedBacks(3,3)
,		   [FourStar] = dbo.fn_NumberOfConferenceFeedBacks(3,4)
,		   [FiveStar] = dbo.fn_NumberOfConferenceFeedBacks(3,5)
from       dbo.ConferenceFeedbacks f
where      (f.Deleted != 1 or f.Deleted is null)
union
select     Q = 4
,          Score = CONVERT(DECIMAL(3,2), AVG(CONVERT(DECIMAL, Question4)))
,		   [OneStar] = dbo.fn_NumberOfConferenceFeedBacks(4,1)
,		   [TwoStar] = dbo.fn_NumberOfConferenceFeedBacks(4,2)
,		   [ThreeStar] = dbo.fn_NumberOfConferenceFeedBacks(4,3)
,		   [FourStar] = dbo.fn_NumberOfConferenceFeedBacks(4,4)
,		   [FiveStar] = dbo.fn_NumberOfConferenceFeedBacks(4,5)
from       dbo.ConferenceFeedbacks f
where      (f.Deleted != 1 or f.Deleted is null)
union
select     Q = 5
,          Score = CONVERT(DECIMAL(3,2), AVG(CONVERT(DECIMAL, Question5)))
,		   [OneStar] = dbo.fn_NumberOfConferenceFeedBacks(5,1)
,		   [TwoStar] = dbo.fn_NumberOfConferenceFeedBacks(5,2)
,		   [ThreeStar] = dbo.fn_NumberOfConferenceFeedBacks(5,3)
,		   [FourStar] = dbo.fn_NumberOfConferenceFeedBacks(5,4)
,		   [FiveStar] = dbo.fn_NumberOfConferenceFeedBacks(5,5)
from       dbo.ConferenceFeedbacks f
where      (f.Deleted != 1 or f.Deleted is null)
union
select     Q = 6
,          Score = CONVERT(DECIMAL(3,2), AVG(CONVERT(DECIMAL, Question6)))
,		   [OneStar] = dbo.fn_NumberOfConferenceFeedBacks(6,1)
,		   [TwoStar] = dbo.fn_NumberOfConferenceFeedBacks(6,2)
,		   [ThreeStar] = dbo.fn_NumberOfConferenceFeedBacks(6,3)
,          [FourStar] = -1
,          [FiveStar] = -1
from       dbo.ConferenceFeedbacks f
where      (f.Deleted != 1 or f.Deleted is null)
union
select     Q = 7
,          Score = CONVERT(DECIMAL(3,2), AVG(CONVERT(DECIMAL, Question7)))
,		   [OneStar] = dbo.fn_NumberOfConferenceFeedBacks(7,1)
,		   [TwoStar] = dbo.fn_NumberOfConferenceFeedBacks(7,2)
,		   [ThreeStar] = dbo.fn_NumberOfConferenceFeedBacks(7,3)
,          [FourStar] = -1
,          [FiveStar] = -1
from       dbo.ConferenceFeedbacks f
where      (f.Deleted != 1 or f.Deleted is null)

