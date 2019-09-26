USE [TechDays2017DB]
GO

/****** Object:  Table [dbo].[ScavengerHuntCompletions]    Script Date: 2017-09-08 14:20:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ScavengerHuntCompletions] (
	[UserId][nvarchar](128) NOT NULL,
	[HuntId][nvarchar](128) NOT NULL,
	[ObjectId][nvarchar](128) NOT NULL,
	[Attempts][int] NOT NULL,
	[CompletedAt] [datetimeoffset](7) NOT NULL,
)

GO

ALTER TABLE [dbo].[ScavengerHuntCompletions] ADD  DEFAULT (sysutcdatetime()) FOR [CompletedAt]
GO

