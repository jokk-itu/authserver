BEGIN
	IF NOT EXISTS (SELECT * FROM Client WHERE [Name] = 'authserver')
	BEGIN
		INSERT INTO Client (Id, [Name], ClientUri, ApplicationType, TokenEndpointAuthMethod, TokenEndpointAuthSigningAlg, CreatedAt, AccessTokenExpiration, RequireConsent, RequirePushedAuthorizationRequests, RequireReferenceToken, RequireSignedRequestObject)
		VALUES (NEWID(), 'authserver', 'https://localhost.7254', 0, 0, 0, GETUTCDATE(), 0, 0, 0, 0, 0)
	END
END