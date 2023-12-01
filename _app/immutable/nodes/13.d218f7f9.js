import{S as $e,i as Te,s as we,y as V,a as T,z as X,c as w,A as Y,b as s,g,d as ee,B as te,h as t,q as p,r as f,k as l,l as a,m as v,n as be,D as b,H as he}from"../chunks/index.8cffed02.js";import{P as ve}from"../chunks/PageTitle.df959b80.js";import{S as fe}from"../chunks/Section.be805b22.js";function ye(B){let i;return{c(){i=p(`Is used to get a structured token, by passing a reference token to the
endpoint. It is used by resources when a client accesses a protected
endpoint, and passes a reference token. It is also used by clients to
check the validity of tokens.`)},l(c){i=f(c,`Is used to get a structured token, by passing a reference token to the
endpoint. It is used by resources when a client accesses a protected
endpoint, and passes a reference token. It is also used by clients to
check the validity of tokens.`)},m(c,r){s(c,i,r)},d(c){c&&t(i)}}}function Re(B){let i,c,r,m;return{c(){i=l("ul"),c=l("li"),r=l("a"),m=p("OAuth Introspection"),this.h()},l(u){i=a(u,"UL",{class:!0});var _=v(i);c=a(_,"LI",{});var k=v(c);r=a(k,"A",{href:!0});var h=v(r);m=f(h,"OAuth Introspection"),h.forEach(t),k.forEach(t),_.forEach(t),this.h()},h(){be(r,"href","https://datatracker.ietf.org/doc/html/rfc7662"),be(i,"class","list-disc")},m(u,_){s(u,i,_),b(i,c),b(c,r),b(r,m)},p:he,d(u){u&&t(i)}}}function Be(B){let i,c,r,m,u,_,k,h,n,d,A,P,$,ne,E,z,O,C,H,S,W,I,se,j,x,F,U,M,y,oe,ue=`{
     "active": false
    }`,ie,re,D,J,K,R,le,de=`{
     "active": true,
     "client_id": "adeaf3f2-3459-4c50-89b5-cc1c446525c5",
     "username": "jdoe",
     "jti": "7690c465-2234-4df2-914d-ca42a2001939",
     "token_type": "access_token",
     "scope": "openid profile weather:read",
     "sub": "75e24e31-647a-4011-8a17-106c7fe82db1",
     "aud": "https://weather.authserver.dk",
     "iss": "https://idp.authserver.dk",
     "exp": 1419356238,
     "iat": 1419350238,
     "nbf": 1419350238
    }`,ae,ce,L,Q,G,q,pe;return{c(){i=p(`Requests are sent using the POST method,
and the request body is encoded as form-urlcencoded.
Authentication is also needed for the requester,
and the endpoint can be utilized by both clients
and protected resources. The authentication method must be supported,
and are listed in the discovery endpoint at introspection_endpoint_auth_methods_supported. 
`),c=l("br"),r=p(`
The following parameters are allowed:`),m=l("br"),u=T(),_=l("b"),k=p("token"),h=l("br"),n=p(`
REQUIRED. A reference token.
`),d=l("br"),A=l("br"),P=T(),$=l("b"),ne=p("token_type_hint"),E=l("br"),z=p(`
OPTIONAL. Can be one of the following values: access_token and refresh_token.
`),O=l("br"),C=l("br"),H=p(`
The request can look like the following using Basic client authentication:`),S=l("br"),W=T(),I=l("pre"),se=p(`    POST /connect/introspect HTTP/1.1
    Host: idp.authserver.dk
    Accept: application/json
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    token=mF9B5f41JqM&token_type_hint=access_token
`),j=T(),x=l("br"),F=p(`
The request is validated by authenticating the caller.
If the protected resource or client is unauthenticated,
a statuscode 401 is returned. If the protected resource
or client is requesting a token, which they are not authorized
to request or it is unknown, a successful response is returned stating the token is inactive.
Unrecognized token_type_hints are ignored completely.
`),U=l("br"),M=p(`
A successful response with an inactive token can look like the following:
`),y=l("pre"),oe=p(`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),ie=p(ue),re=p(`
`),D=T(),J=l("br"),K=p(`
A successful response with an active token can look like the following:
`),R=l("pre"),le=p(`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),ae=p(de),ce=p(`
`),L=T(),Q=l("br"),G=p(`
An error response for an unauthenticated protected resource or client can look like the following:
`),q=l("pre"),pe=p(`    HTTP/1.1 401 Unauthorized
    WWW-Authenticate: Bearer error="invalid_token",
                      error_description="The access token expired"
`)},l(e){i=f(e,`Requests are sent using the POST method,
and the request body is encoded as form-urlcencoded.
Authentication is also needed for the requester,
and the endpoint can be utilized by both clients
and protected resources. The authentication method must be supported,
and are listed in the discovery endpoint at introspection_endpoint_auth_methods_supported. 
`),c=a(e,"BR",{}),r=f(e,`
The following parameters are allowed:`),m=a(e,"BR",{}),u=w(e),_=a(e,"B",{});var o=v(_);k=f(o,"token"),o.forEach(t),h=a(e,"BR",{}),n=f(e,`
REQUIRED. A reference token.
`),d=a(e,"BR",{}),A=a(e,"BR",{}),P=w(e),$=a(e,"B",{});var _e=v($);ne=f(_e,"token_type_hint"),_e.forEach(t),E=a(e,"BR",{}),z=f(e,`
OPTIONAL. Can be one of the following values: access_token and refresh_token.
`),O=a(e,"BR",{}),C=a(e,"BR",{}),H=f(e,`
The request can look like the following using Basic client authentication:`),S=a(e,"BR",{}),W=w(e),I=a(e,"PRE",{});var ke=v(I);se=f(ke,`    POST /connect/introspect HTTP/1.1
    Host: idp.authserver.dk
    Accept: application/json
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    token=mF9B5f41JqM&token_type_hint=access_token
`),ke.forEach(t),j=w(e),x=a(e,"BR",{}),F=f(e,`
The request is validated by authenticating the caller.
If the protected resource or client is unauthenticated,
a statuscode 401 is returned. If the protected resource
or client is requesting a token, which they are not authorized
to request or it is unknown, a successful response is returned stating the token is inactive.
Unrecognized token_type_hints are ignored completely.
`),U=a(e,"BR",{}),M=f(e,`
A successful response with an inactive token can look like the following:
`),y=a(e,"PRE",{});var N=v(y);oe=f(N,`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),ie=f(N,ue),re=f(N,`
`),N.forEach(t),D=w(e),J=a(e,"BR",{}),K=f(e,`
A successful response with an active token can look like the following:
`),R=a(e,"PRE",{});var Z=v(R);le=f(Z,`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),ae=f(Z,de),ce=f(Z,`
`),Z.forEach(t),L=w(e),Q=a(e,"BR",{}),G=f(e,`
An error response for an unauthenticated protected resource or client can look like the following:
`),q=a(e,"PRE",{});var me=v(q);pe=f(me,`    HTTP/1.1 401 Unauthorized
    WWW-Authenticate: Bearer error="invalid_token",
                      error_description="The access token expired"
`),me.forEach(t)},m(e,o){s(e,i,o),s(e,c,o),s(e,r,o),s(e,m,o),s(e,u,o),s(e,_,o),b(_,k),s(e,h,o),s(e,n,o),s(e,d,o),s(e,A,o),s(e,P,o),s(e,$,o),b($,ne),s(e,E,o),s(e,z,o),s(e,O,o),s(e,C,o),s(e,H,o),s(e,S,o),s(e,W,o),s(e,I,o),b(I,se),s(e,j,o),s(e,x,o),s(e,F,o),s(e,U,o),s(e,M,o),s(e,y,o),b(y,oe),b(y,ie),b(y,re),s(e,D,o),s(e,J,o),s(e,K,o),s(e,R,o),b(R,le),b(R,ae),b(R,ce),s(e,L,o),s(e,Q,o),s(e,G,o),s(e,q,o),b(q,pe)},p:he,d(e){e&&t(i),e&&t(c),e&&t(r),e&&t(m),e&&t(u),e&&t(_),e&&t(h),e&&t(n),e&&t(d),e&&t(A),e&&t(P),e&&t($),e&&t(E),e&&t(z),e&&t(O),e&&t(C),e&&t(H),e&&t(S),e&&t(W),e&&t(I),e&&t(j),e&&t(x),e&&t(F),e&&t(U),e&&t(M),e&&t(y),e&&t(D),e&&t(J),e&&t(K),e&&t(R),e&&t(L),e&&t(Q),e&&t(G),e&&t(q)}}}function Ae(B){let i,c,r,m,u,_,k,h;return i=new ve({props:{title:"Introspection"}}),r=new fe({props:{title:"Introduction",$$slots:{default:[ye]},$$scope:{ctx:B}}}),u=new fe({props:{title:"Specifications",$$slots:{default:[Re]},$$scope:{ctx:B}}}),k=new fe({props:{title:"Introspection",$$slots:{default:[Be]},$$scope:{ctx:B}}}),{c(){V(i.$$.fragment),c=T(),V(r.$$.fragment),m=T(),V(u.$$.fragment),_=T(),V(k.$$.fragment)},l(n){X(i.$$.fragment,n),c=w(n),X(r.$$.fragment,n),m=w(n),X(u.$$.fragment,n),_=w(n),X(k.$$.fragment,n)},m(n,d){Y(i,n,d),s(n,c,d),Y(r,n,d),s(n,m,d),Y(u,n,d),s(n,_,d),Y(k,n,d),h=!0},p(n,[d]){const A={};d&1&&(A.$$scope={dirty:d,ctx:n}),r.$set(A);const P={};d&1&&(P.$$scope={dirty:d,ctx:n}),u.$set(P);const $={};d&1&&($.$$scope={dirty:d,ctx:n}),k.$set($)},i(n){h||(g(i.$$.fragment,n),g(r.$$.fragment,n),g(u.$$.fragment,n),g(k.$$.fragment,n),h=!0)},o(n){ee(i.$$.fragment,n),ee(r.$$.fragment,n),ee(u.$$.fragment,n),ee(k.$$.fragment,n),h=!1},d(n){te(i,n),n&&t(c),te(r,n),n&&t(m),te(u,n),n&&t(_),te(k,n)}}}class Ee extends $e{constructor(i){super(),Te(this,i,null,Ae,we,{})}}export{Ee as component};
