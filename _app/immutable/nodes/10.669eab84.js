import{S,i as m,s as A,y as N,a as R,z as o,c as l,A as I,b as g,g as a,d as C,B as d,h as O,q as P,r as p,H as c}from"../chunks/index.8cffed02.js";import{D as u}from"../chunks/Diagram.ad75e5b6.js";import{P as L}from"../chunks/PageTitle.df959b80.js";function f(E){let n=`
        erDiagram
        TOKEN {
            guid Id PK
            string Reference
            string Scope
            string TokenType
            datetime ExpiresAt
            datetime IssuedAt
            datetime NotBefore
            string Audience
            string Issuer
            datetime RevokedAt
        }
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
            datetime IssuedAt
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

        TOKEN }o--|| CLIENT : ""
        TOKEN }o--|| AUTHORIZATIONCODEGRANT : ""
    `,i;return{c(){i=P(n)},l(e){i=p(e,n)},m(e,s){g(e,i,s)},p:c,d(e){e&&O(i)}}}function U(E){let n,i,e,s;return n=new L({props:{title:"DataModel"}}),e=new u({props:{$$slots:{default:[f]},$$scope:{ctx:E}}}),{c(){N(n.$$.fragment),i=R(),N(e.$$.fragment)},l(t){o(n.$$.fragment,t),i=l(t),o(e.$$.fragment,t)},m(t,r){I(n,t,r),g(t,i,r),I(e,t,r),s=!0},p(t,[r]){const T={};r&1&&(T.$$scope={dirty:r,ctx:t}),e.$set(T)},i(t){s||(a(n.$$.fragment,t),a(e.$$.fragment,t),s=!0)},o(t){C(n.$$.fragment,t),C(e.$$.fragment,t),s=!1},d(t){d(n,t),t&&O(i),d(e,t)}}}class K extends S{constructor(n){super(),m(this,n,null,U,A,{})}}export{K as component};
