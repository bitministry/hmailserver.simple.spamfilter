USE [hmail_]
GO

/****** Object:  Table [dbo].[bitministry_spam_filter ]    Script Date: 2/15/2020 8:52:17 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[bitministry_spam_filter ](
	[spam_filter] [varchar](22) NOT NULL,
	[spam_value] [varchar](222) NOT NULL,
	[created] [datetime] NOT NULL,
	[author] [varchar](222) NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[bitministry_spam_filter ] ADD  CONSTRAINT [DF_bitmin_spam_filter_created]  DEFAULT (getdate()) FOR [created]
GO




CREATE proc [dbo].[sp_check_spam]
	@newSpamFilter varchar(22), 
	@newSpamFilterValue varchar(222), 
	@fromEmail varchar(222), 
	@subject varchar(222), 
	@body ntext,
	@replyTo varchar(222) = null 
as 

/* C# 
		sql.AddWithValue("@newSpamFilter", newSpamFilter);
		sql.AddWithValue("@newSpamFilterValue", newSpamFilterValue);
		sql.AddWithValue("@fromEmail", oMessage.FromAddress);
		sql.AddWithValue("@subject", oMessage.FromAddress);
		sql.AddWithValue("@body", oMessage.FromAddress);
		oMessage.Subject = sql.ExecuteScalar("sp_check_spam") + oMessage.Subject;

		*/

		
	-- replace LIKE pattern reserved characters 
	set @newSpamFilterValue = lower( replace ( replace ( replace ( replace ( @newSpamFilterValue, '[', '' ), ']', '' ), '_', '' ), '%', '' ) )

	if ( len (@newSpamFilter) > 0 and len (@newSpamFilterValue) > 0  )
	begin
		print 'adding new filter'
		if not exists (  select * from bitministry_spam_filter where spam_filter = @newSpamFilter and spam_value = @newSpamFilterValue ) 
			insert into bitministry_spam_filter (spam_filter, spam_value, author) values (@newSpamFilter, @newSpamFilterValue, @fromEmail )
		return 
	end 

	if exists ( select * from bitministry_spam_filter where spam_filter = 'senderToSpam' and ( spam_value = @fromEmail or spam_value = @replyTo ) )
		select '*bitSpam* sender: '

	if exists ( select * from bitministry_spam_filter where spam_filter = 'domainToSpam' and ( @fromEmail like '%'+ spam_value or @replyTo like '%'+ spam_value ) )
		select '*bitSpam* DOMAIN: '

	if exists ( select * from bitministry_spam_filter where spam_filter = 'subjectToSpam' and @subject like '%'+ spam_value + '%' )
		select '*bitSpam* subject: '

	if exists ( select * from bitministry_spam_filter where spam_filter = 'bodyToSpam' and @body like '%'+ spam_value + '%')
		select '*bitSpam* body: '



GO

