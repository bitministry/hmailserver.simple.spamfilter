Option Explicit

dim spamHandler 

Sub OnSpamHandler(oMessage)
	
	
	if ( isobject ( spamHandler ) = false ) then 
		Set spamHandler = CreateObject("BitMinistry.hMailServer.SpamHandler.Handler") 
		spamHandler.SetConfigFile "D:\BitMinistry.hMailServer.SpamHandler\BitMinistry.hMailServer.SpamHandler.dll.config" 
	end if 
	
	spamHandler.Check oMessage 

End Sub
