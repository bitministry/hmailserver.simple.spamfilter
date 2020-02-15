# hmailserver.simple.spamfilter
simple local spam filter for hMailServer


# USAGE 

To add SENDER RULE as spammer, **simply forward the email** to configured address (eg spam-sender@mydomain.com) 
All incoming emails where sender address EQUALS "bob.thespammer@gmail.com" will be moved to Trash 

To add DOMAIN RULE as spammer, **simply forward the email** to configured address (eg spam-domain@mydomain.com)
All incoming emails where sender address TSQL LIKE "%spammerdomain.com" will be moved to Trash 

To add SUBJECT RULE, **trim the line in subject and forward the email** (eg spam-subject@mydomain.com)
All incoming emails where subject CONTAINS "<trimmed string from subject>" will be moved to Trash 

To add BODY RULE, **trim the line in body and forward the email** (eg spam-body@mydomain.com)
All incoming emails where body CONTAINS "<trimmed string from body>" will be moved to Trash 


### register ActiveX (TLB)

	Framework\v4.0.30319>RegAsm.exe ????.dll /register /codebase /tlb

unregister
	Framework\v4.0.30319>RegAsm.exe ????.dll /u 
	
when you change .config file
	you need to restart hMailServer 
	no need to re-register TLB

when you change hMailServer/Events/EventHandlers.vbs
	hMailserver>settings>advanced>scripts>reload scripts 


### BitMinistry.hMailServer.SpamHandler.dll.config

  <connectionStrings>
    <add name="main"
         connectionString="/////////////////////// hmail mssql connection string ////////////////////////"
         providerName="System.Data.SqlClient" />
  </connectionStrings>

  <appSettings>

    <add key="senderToSpam" value="spam-sender@hetk.ee" />
    <add key="domainToSpam" value="spam-domain@hetk.ee" />
    <add key="subjectToSpam" value="spam-subject@hetk.ee" />
    <add key="bodyToSpam" value="spam-body@hetk.ee" />

    <add key="debug" value="true" /> 

*debug entries will be added Event Viewer > Application log*

    <add key="addReplyPathToBottom" value="true" />

  </appSettings>


### MSSQL setup
```tsql

CREATE TABLE [dbo].[bitministry_spam_filter ](
	[spam_filter] [varchar](22) NOT NULL,
	[spam_value] [varchar](222) NOT NULL,
	[created] [datetime] NOT NULL,
	[author] [varchar](222) NULL
) 

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

```
