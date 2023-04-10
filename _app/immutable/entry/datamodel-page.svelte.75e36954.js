import{S as O,i as R,s as m,y as T,a as A,z as a,c as l,A as C,b as g,g as I,d as o,B as d,h as S,q as P,r as p,H as c}from"../chunks/index.2ef5bca6.js";import{D as u}from"../chunks/Diagram.9ecf90b9.js";import{P as L}from"../chunks/PageTitle.4f3ee9c3.js";function f(E){let n=`
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
    `,i;return{c(){i=P(n)},l(e){i=p(e,n)},m(e,s){g(e,i,s)},p:c,d(e){e&&S(i)}}}function U(E){let n,i,e,s;return n=new L({props:{title:"DataModel"}}),e=new u({props:{$$slots:{default:[f]},$$scope:{ctx:E}}}),{c(){T(n.$$.fragment),i=A(),T(e.$$.fragment)},l(t){a(n.$$.fragment,t),i=l(t),a(e.$$.fragment,t)},m(t,r){C(n,t,r),g(t,i,r),C(e,t,r),s=!0},p(t,[r]){const N={};r&1&&(N.$$scope={dirty:r,ctx:t}),e.$set(N)},i(t){s||(I(n.$$.fragment,t),I(e.$$.fragment,t),s=!0)},o(t){o(n.$$.fragment,t),o(e.$$.fragment,t),s=!1},d(t){d(n,t),t&&S(i),d(e,t)}}}class K extends O{constructor(n){super(),R(this,n,null,U,m,{})}}export{K as default};
