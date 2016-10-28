if exists (select 1 from dbo.sysobjects where type = 'FN' and name = 'fn_NumberOfConferenceFeedBacks')
begin
	drop function dbo.[fn_NumberOfConferenceFeedBacks]
end
go


create function [dbo].[fn_NumberOfConferenceFeedBacks]
(
	@p_Question INT
,	@p_Rating INT
)
returns INT
as
begin
	declare @num INT

	if (@p_Question = 1)
	begin
		select @num = count(1)
		from dbo.ConferenceFeedbacks
		where Question1 = @p_Rating
	end
	if (@p_Question = 2)
	begin
		select @num = count(1)
		from dbo.ConferenceFeedbacks
		where Question2 = @p_Rating
	end
	if (@p_Question = 3)
	begin
		select @num = count(1)
		from dbo.ConferenceFeedbacks
		where Question3 = @p_Rating
	end
	if (@p_Question = 4)
	begin
		select @num = count(1)
		from dbo.ConferenceFeedbacks
		where Question4 = @p_Rating
	end
	if (@p_Question = 5)
	begin
		select @num = count(1)
		from dbo.ConferenceFeedbacks
		where Question5 = @p_Rating
	end
	if (@p_Question = 6)
	begin
		select @num = count(1)
		from dbo.ConferenceFeedbacks
		where Question6 = @p_Rating
	end
	if (@p_Question = 7)
	begin
		select @num = count(1)
		from dbo.ConferenceFeedbacks
		where Question7 = @p_Rating
	end
	if (@p_Question = 8)
	begin
		select @num = count(1)
		from dbo.ConferenceFeedbacks
		where Question8 = @p_Rating
	end
	if (@p_Question = 9)
	begin
		select @num = count(1)
		from dbo.ConferenceFeedbacks
		where Question9 = @p_Rating
	end
	if (@p_Question = 10)
	begin
		select @num = count(1)
		from dbo.ConferenceFeedbacks
		where Question10 = @p_Rating
	end

	return @num
end

GO

grant execute on dbo.[fn_NumberOfConferenceFeedBacks] to public
go