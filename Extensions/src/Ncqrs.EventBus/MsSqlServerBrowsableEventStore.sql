﻿CREATE TABLE [dbo].[PipelineState](
	[BatchId] [int] IDENTITY(1,1) NOT NULL,
	[LastProcessedEventId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_MainPipelineState] PRIMARY KEY CLUSTERED 
(
	[BatchId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO