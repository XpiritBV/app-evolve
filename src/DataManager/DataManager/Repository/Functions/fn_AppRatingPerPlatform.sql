if exists (select 1 from dbo.sysobjects where type = 'FN' and name = 'fn_NumberOfRatings')
begin
	drop function dbo.fn_AppRatingPerPlatform
end
go


create function [dbo].fn_AppRatingPerPlatform
(
	@p_Rating INT
,   @p_Platform NVARCHAR(20)
)
returns INT
as
begin
	declare @num INT

	select @num = count(1)
	from dbo.ConferenceFeedbacks
	where Question5 = @p_Rating
	and   DeviceOS = @p_Platform

	return @num
end

GO

grant execute on dbo.fn_AppRatingPerPlatform to public
go