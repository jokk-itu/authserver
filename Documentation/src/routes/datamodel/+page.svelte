<script>
    import Diagram from "../../components/Diagram.svelte";
    import PageTitle from "../../components/PageTitle.svelte";

</script>

<PageTitle title="DataModel" />


<Diagram>
    {`
        erDiagram
        CLIENT {
            string Id PK
            string Name
            string Secret
            string TosUri
            string PolicyUri
            string ClientUri
            string LogoUri
            string InitiateLoginUri
            long DefaultMaxAge
            string ApplicationType
            string TokenEndpointAuthMethod
            string SubjectType
        }
        SESSION {
            string Id PK
            bool IsRevoked
        }
        SESSIONCLIENT {
            string ClientId PK, FK
            string SessionId PK, FK
        }
        CLIENTSCOPE {
            string ClientId PK, FK
            int ScopeId PK, FK
        }
        CONSENTEDGRANTCLAIM {
            int ConsentGrantId PK, FK
            int ConsentedClaimsId PK, FK
        }
        CONSENTEDGRANTSCOPE {
            int ConsentGrantId 
            int ConsentedScopeId 
        }
        CONSENTGRANT {
            int Id 
            datetime Updated
            string ClientId 
            int UserId 
        }
        CONTACT {
            int Id 
            string Email
        }
        GRANTTYPE {
            int Id 
            string Name
        }
        CLAIM {
            int Id 
            string Name
        }
        CLIENTCONTACT {
            string ClientId 
            int ContactId 
        }
        CLIENTGRANTTYPE {
            string ClientId 
            int GrantTypeId 
        }
        CLIENTRESPONSETYPE {
            string ClientId 
            int ResponseTypeId 
        }
        JWK {
            long KeyId 
            datetime CreatedTimeStamp
            bytes PrivateKey
            bytes Modulus
            bytes Exponent
        }
        RESPONSETYPE {
            int Id 
            string Name
        }
        REDIRECTURI {
            int Id 
            string Uri
            string Type
            string ClientId 
        }
        RESOURCE {
            string Id 
            string Name
            string Secret
        }
        RESOURCESCOPE {
            int ResourceId 
            int ScopeId 
        }
        SCOPE {
            int Id 
            string Name
        }
        AUTHORIZATIONCODE {
            string Id 
            string Value
            bool IsRedeemed
            datetime IssuedAt
            datetime RedeemedAt
            string AuthorizationCodeGrantId 
        }
        NONCE {
            string Id 
            string Value
            string AuthorizationCodeGrantId 
        }
        AUTHORIZATIONCODEGRANT {
            string Id 
            datetime AuthTime
            bool IsRevoked
            string SessionId 
            string ClientId 
        }
        USER {
            string Id 
            string UserName
            string Password
            string PhoneNumber
            string Email
            bool IsEmailVerified
            bool IsPhoneNumberVerified
            string Address
            string LastName
            string FirstName
            DateTime Birthdate
            string Locale
        }
    
        RESOURCE ||--o{ RESOURCESCOPE : ""
    
        CONTACT ||--o{ CLIENTCONTACT : ""
    
        USER ||--o{ CONSENTGRANT : ""
        USER ||--o{ SESSION : ""
    
        AUTHORIZATIONCODEGRANT }|--|| SESSION : ""
        AUTHORIZATIONCODEGRANT }o--|| CLIENT : ""
        AUTHORIZATIONCODEGRANT ||--|{ NONCE : ""
        AUTHORIZATIONCODEGRANT ||--|{ AUTHORIZATIONCODE : ""
    
        CLIENT ||--|{ REDIRECTURI : ""
        CLIENT ||--|{ CLIENTRESPONSETYPE : ""
        CLIENT ||--|{ CLIENTGRANTTYPE : ""
        CLIENT ||--o{ CLIENTCONTACT : ""
        CLIENT ||--o{ SESSIONCLIENT : ""
        CLIENT ||--o{ CLIENTSCOPE : ""
    
        CLIENTRESPONSETYPE }|--|| RESPONSETYPE : ""
    
        CLIENTGRANTTYPE }|--|| GRANTTYPE : ""
    
        CONSENTGRANT ||--o{ CONSENTEDGRANTCLAIM : ""
        CONSENTGRANT ||--|{ CONSENTEDGRANTSCOPE : ""
    
        CLAIM ||--o{ CONSENTEDGRANTCLAIM : ""
    
        SCOPE ||--o{ CONSENTEDGRANTSCOPE : ""
        SCOPE ||--o{ CLIENTSCOPE : ""
        SCOPE ||--o{ RESOURCESCOPE : ""
    `}
    </Diagram>