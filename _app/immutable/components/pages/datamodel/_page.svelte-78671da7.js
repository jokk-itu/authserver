import{S,i as m,s as A,c as N,a as R,b as I,d as l,m as a,k as g,o,p as C,q as d,j as O,t as P,h as p,n as u}from"../../../chunks/index-8ff2e5dd.js";import{D as c}from"../../../chunks/Diagram-adaf7575.js";import{P as L}from"../../../chunks/PageTitle-edc21ece.js";function f(E){let i=`
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
    `,n;return{c(){n=P(i)},l(e){n=p(e,i)},m(e,s){g(e,n,s)},p:u,d(e){e&&O(n)}}}function U(E){let i,n,e,s;return i=new L({props:{title:"DataModel"}}),e=new c({props:{$$slots:{default:[f]},$$scope:{ctx:E}}}),{c(){N(i.$$.fragment),n=R(),N(e.$$.fragment)},l(t){I(i.$$.fragment,t),n=l(t),I(e.$$.fragment,t)},m(t,r){a(i,t,r),g(t,n,r),a(e,t,r),s=!0},p(t,[r]){const T={};r&1&&(T.$$scope={dirty:r,ctx:t}),e.$set(T)},i(t){s||(o(i.$$.fragment,t),o(e.$$.fragment,t),s=!0)},o(t){C(i.$$.fragment,t),C(e.$$.fragment,t),s=!1},d(t){d(i,t),t&&O(n),d(e,t)}}}class K extends S{constructor(i){super(),m(this,i,null,U,A,{})}}export{K as default};
