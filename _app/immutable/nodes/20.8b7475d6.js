import{S as wt,i as yt,s as Bt,y as qe,a as v,z as Me,c as E,A as xe,b as n,g as We,d as Ge,B as Je,h as t,q as l,r as f,k as r,l as s,m as d,n as lt,D as p,H as Tt}from"../chunks/index.8cffed02.js";import{P as $t}from"../chunks/PageTitle.df959b80.js";import{S as ft}from"../chunks/Section.be805b22.js";function It(I){let a;return{c(){a=l("The token endpoint is used by the OP to issue access, refresh and id tokens to RPs by redeeming grants.")},l(_){a=f(_,"The token endpoint is used by the OP to issue access, refresh and id tokens to RPs by redeeming grants.")},m(_,b){n(_,a,b)},d(_){_&&t(a)}}}function zt(I){let a,_,b,h,m,R,c,k;return{c(){a=r("ul"),_=r("li"),b=r("a"),h=l("OIDC 1.0"),m=v(),R=r("li"),c=r("a"),k=l("OAuth 2.1"),this.h()},l(o){a=s(o,"UL",{class:!0});var u=d(a);_=s(u,"LI",{});var w=d(_);b=s(w,"A",{href:!0});var y=d(b);h=f(y,"OIDC 1.0"),y.forEach(t),w.forEach(t),m=E(u),R=s(u,"LI",{});var T=d(R);c=s(T,"A",{href:!0});var z=d(c);k=f(z,"OAuth 2.1"),z.forEach(t),T.forEach(t),u.forEach(t),this.h()},h(){lt(b,"href","https://openid.net/specs/openid-connect-core-1_0.html"),lt(c,"href","https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-09"),lt(a,"class","list-disc")},m(o,u){n(o,a,u),p(a,_),p(_,b),p(b,h),p(a,m),p(a,R),p(R,c),p(c,k)},p:Tt,d(o){o&&t(a)}}}function Pt(I){let a,_,b,h,m,R,c,k,o,u,w,y,T,z,q,M,x,W,G,P,Ze,J,Z,j,K,L,D,je,N,V,X,Y,g,U,Ke,ee,te,ne,ie,re,Q,Le,se,oe,le,fe,ae,O,Ne,ue,pe,be,_e,ce,H,Ve,de,me,Re,he,ke,C,Xe,ve,Ee,Te,we,ye,S,Ye,Be,$e,Ie,F,ge,ze,Pe,De,B,et,at=`{
     "access_token": "aiseubisadvsdbgur",
     "expires_in": 3600,
     "refresh_token": "lauribvidbvdfv";
     "id_token": "aleryubvksjdv",
     "scope": "openid identityprovider:userinfo"
    }`,tt,nt,Ue,Qe,Oe,A,it,He,Ce,Se,$,rt,ut=`{
     "access_token": "aiseubisadvsdbgur",
     "expires_in": 3600,
     "scope": "weather:read"
    }`,st,ot;return{c(){a=l(`Requests are sent using the POST method,
and the body is encoded as form-urlencoded.
The client must also authenticate itself,
using the authentication method registered in the token_endpoint_auth_method.`),_=r("br"),b=l(`
The following parameters are allowed:`),h=r("br"),m=v(),R=r("b"),c=l("grant_type"),k=r("br"),o=l(`
REQUIRED. The grant being redeemed for token(s).
The supported grants can be found at the discovery endpoint under grant_types.
`),u=r("br"),w=r("br"),y=v(),T=r("b"),z=l("code"),q=r("br"),M=l(`
REQUIRED. If the grant_type is authorization_code.
It is the authorization_code issued by the OP.
`),x=r("br"),W=r("br"),G=v(),P=r("b"),Ze=l("client_id"),J=r("br"),Z=l(`
REQUIRED. Used to authenticate the client.
How it is sent is determined by the client authentication method.
`),j=r("br"),K=r("br"),L=v(),D=r("b"),je=l("client_secret"),N=r("br"),V=l(`
REQUIRED. Used to authenticate the client.
How it is sent is determined by the client authenticatino method.
`),X=r("br"),Y=r("br"),g=v(),U=r("b"),Ke=l("redirect_uri"),ee=r("br"),te=l(`
REQUIRED. If the grant_type is authorization_code
and the authorize request included a redirect_uri parameter.
If included, it must match a registered redirect_uri,
and it must match the same redirect_uri given at the authorize endpoint, if one was provided.
`),ne=r("br"),ie=r("br"),re=v(),Q=r("b"),Le=l("scope"),se=r("br"),oe=l(`
REQUIRED. If the grant_type is client_credentials.
It can be useful during refresh_token grant,
if the access token should only contain a subset of the authorized scope in the initial grant.
If not provided, the consented scope from the grant is used, as long as it is registered at a resource identified by the resource parameter.
`),le=r("br"),fe=r("br"),ae=v(),O=r("b"),Ne=l("code_verifier"),ue=r("br"),pe=l(`
REQUIRED. If the grant_type is authorization_code.
It will be validated against the code, since it contains the code_challenge from the authorize request.
It must be between 43 and 128 characters long.
`),be=r("br"),_e=r("br"),ce=v(),H=r("b"),Ve=l("refresh_token"),de=r("br"),me=l(`
REQUIRED. If the grant_type is refresh_token.
It will be used and then invalidated. A new refresh_token is returned in the response.
`),Re=r("br"),he=r("br"),ke=v(),C=r("b"),Xe=l("resource"),ve=r("br"),Ee=l(`
REQUIRED. It will be used as the audience of the access_token returned in the response.
Multiple resource parameters can be given and each must be a resource Uri registered at the OP.
Any resource must also expect at least one provided scope.
`),Te=r("br"),we=r("br"),ye=l(`
The request for authorization_code grant can look like the following:
`),S=r("pre"),Ye=l(`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=authorization_code
    &code=fgukaoirnenvsoidnv
    &redirect_uri=https://webapp.authserver.dk/signin-callback
    &code_verifier=saeoginsoivn...
    &resource=https://weather.authserver.dk
`),Be=r("br"),$e=r("br"),Ie=l(`
The request for refresh_token grant can look like the following:
`),F=r("pre"),ge=l(`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=refresh_token
    &refresh_token=fgukaoirnenvsoidnv
    &scope=openid%20identityprovider:userinfo
`),ze=r("br"),Pe=r("br"),De=l(`
The response for authorization_code and refresh_token grant can look like the following:
`),B=r("pre"),et=l(`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),tt=l(at),nt=l(`
`),Ue=r("br"),Qe=r("br"),Oe=l(`
The request for client_credentials grant can look like the following:
`),A=r("pre"),it=l(`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=client_credentials&scope=weather%3Aread
`),He=r("br"),Ce=r("br"),Se=l(`
The response for client_credentials grant can look like the following:
`),$=r("pre"),rt=l(`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),st=l(ut),ot=l(`
`)},l(e){a=f(e,`Requests are sent using the POST method,
and the body is encoded as form-urlencoded.
The client must also authenticate itself,
using the authentication method registered in the token_endpoint_auth_method.`),_=s(e,"BR",{}),b=f(e,`
The following parameters are allowed:`),h=s(e,"BR",{}),m=E(e),R=s(e,"B",{});var i=d(R);c=f(i,"grant_type"),i.forEach(t),k=s(e,"BR",{}),o=f(e,`
REQUIRED. The grant being redeemed for token(s).
The supported grants can be found at the discovery endpoint under grant_types.
`),u=s(e,"BR",{}),w=s(e,"BR",{}),y=E(e),T=s(e,"B",{});var pt=d(T);z=f(pt,"code"),pt.forEach(t),q=s(e,"BR",{}),M=f(e,`
REQUIRED. If the grant_type is authorization_code.
It is the authorization_code issued by the OP.
`),x=s(e,"BR",{}),W=s(e,"BR",{}),G=E(e),P=s(e,"B",{});var bt=d(P);Ze=f(bt,"client_id"),bt.forEach(t),J=s(e,"BR",{}),Z=f(e,`
REQUIRED. Used to authenticate the client.
How it is sent is determined by the client authentication method.
`),j=s(e,"BR",{}),K=s(e,"BR",{}),L=E(e),D=s(e,"B",{});var _t=d(D);je=f(_t,"client_secret"),_t.forEach(t),N=s(e,"BR",{}),V=f(e,`
REQUIRED. Used to authenticate the client.
How it is sent is determined by the client authenticatino method.
`),X=s(e,"BR",{}),Y=s(e,"BR",{}),g=E(e),U=s(e,"B",{});var ct=d(U);Ke=f(ct,"redirect_uri"),ct.forEach(t),ee=s(e,"BR",{}),te=f(e,`
REQUIRED. If the grant_type is authorization_code
and the authorize request included a redirect_uri parameter.
If included, it must match a registered redirect_uri,
and it must match the same redirect_uri given at the authorize endpoint, if one was provided.
`),ne=s(e,"BR",{}),ie=s(e,"BR",{}),re=E(e),Q=s(e,"B",{});var dt=d(Q);Le=f(dt,"scope"),dt.forEach(t),se=s(e,"BR",{}),oe=f(e,`
REQUIRED. If the grant_type is client_credentials.
It can be useful during refresh_token grant,
if the access token should only contain a subset of the authorized scope in the initial grant.
If not provided, the consented scope from the grant is used, as long as it is registered at a resource identified by the resource parameter.
`),le=s(e,"BR",{}),fe=s(e,"BR",{}),ae=E(e),O=s(e,"B",{});var mt=d(O);Ne=f(mt,"code_verifier"),mt.forEach(t),ue=s(e,"BR",{}),pe=f(e,`
REQUIRED. If the grant_type is authorization_code.
It will be validated against the code, since it contains the code_challenge from the authorize request.
It must be between 43 and 128 characters long.
`),be=s(e,"BR",{}),_e=s(e,"BR",{}),ce=E(e),H=s(e,"B",{});var Rt=d(H);Ve=f(Rt,"refresh_token"),Rt.forEach(t),de=s(e,"BR",{}),me=f(e,`
REQUIRED. If the grant_type is refresh_token.
It will be used and then invalidated. A new refresh_token is returned in the response.
`),Re=s(e,"BR",{}),he=s(e,"BR",{}),ke=E(e),C=s(e,"B",{});var ht=d(C);Xe=f(ht,"resource"),ht.forEach(t),ve=s(e,"BR",{}),Ee=f(e,`
REQUIRED. It will be used as the audience of the access_token returned in the response.
Multiple resource parameters can be given and each must be a resource Uri registered at the OP.
Any resource must also expect at least one provided scope.
`),Te=s(e,"BR",{}),we=s(e,"BR",{}),ye=f(e,`
The request for authorization_code grant can look like the following:
`),S=s(e,"PRE",{});var kt=d(S);Ye=f(kt,`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=authorization_code
    &code=fgukaoirnenvsoidnv
    &redirect_uri=https://webapp.authserver.dk/signin-callback
    &code_verifier=saeoginsoivn...
    &resource=https://weather.authserver.dk
`),kt.forEach(t),Be=s(e,"BR",{}),$e=s(e,"BR",{}),Ie=f(e,`
The request for refresh_token grant can look like the following:
`),F=s(e,"PRE",{});var vt=d(F);ge=f(vt,`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=refresh_token
    &refresh_token=fgukaoirnenvsoidnv
    &scope=openid%20identityprovider:userinfo
`),vt.forEach(t),ze=s(e,"BR",{}),Pe=s(e,"BR",{}),De=f(e,`
The response for authorization_code and refresh_token grant can look like the following:
`),B=s(e,"PRE",{});var Fe=d(B);et=f(Fe,`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),tt=f(Fe,at),nt=f(Fe,`
`),Fe.forEach(t),Ue=s(e,"BR",{}),Qe=s(e,"BR",{}),Oe=f(e,`
The request for client_credentials grant can look like the following:
`),A=s(e,"PRE",{});var Et=d(A);it=f(Et,`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=client_credentials&scope=weather%3Aread
`),Et.forEach(t),He=s(e,"BR",{}),Ce=s(e,"BR",{}),Se=f(e,`
The response for client_credentials grant can look like the following:
`),$=s(e,"PRE",{});var Ae=d($);rt=f(Ae,`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),st=f(Ae,ut),ot=f(Ae,`
`),Ae.forEach(t)},m(e,i){n(e,a,i),n(e,_,i),n(e,b,i),n(e,h,i),n(e,m,i),n(e,R,i),p(R,c),n(e,k,i),n(e,o,i),n(e,u,i),n(e,w,i),n(e,y,i),n(e,T,i),p(T,z),n(e,q,i),n(e,M,i),n(e,x,i),n(e,W,i),n(e,G,i),n(e,P,i),p(P,Ze),n(e,J,i),n(e,Z,i),n(e,j,i),n(e,K,i),n(e,L,i),n(e,D,i),p(D,je),n(e,N,i),n(e,V,i),n(e,X,i),n(e,Y,i),n(e,g,i),n(e,U,i),p(U,Ke),n(e,ee,i),n(e,te,i),n(e,ne,i),n(e,ie,i),n(e,re,i),n(e,Q,i),p(Q,Le),n(e,se,i),n(e,oe,i),n(e,le,i),n(e,fe,i),n(e,ae,i),n(e,O,i),p(O,Ne),n(e,ue,i),n(e,pe,i),n(e,be,i),n(e,_e,i),n(e,ce,i),n(e,H,i),p(H,Ve),n(e,de,i),n(e,me,i),n(e,Re,i),n(e,he,i),n(e,ke,i),n(e,C,i),p(C,Xe),n(e,ve,i),n(e,Ee,i),n(e,Te,i),n(e,we,i),n(e,ye,i),n(e,S,i),p(S,Ye),n(e,Be,i),n(e,$e,i),n(e,Ie,i),n(e,F,i),p(F,ge),n(e,ze,i),n(e,Pe,i),n(e,De,i),n(e,B,i),p(B,et),p(B,tt),p(B,nt),n(e,Ue,i),n(e,Qe,i),n(e,Oe,i),n(e,A,i),p(A,it),n(e,He,i),n(e,Ce,i),n(e,Se,i),n(e,$,i),p($,rt),p($,st),p($,ot)},p:Tt,d(e){e&&t(a),e&&t(_),e&&t(b),e&&t(h),e&&t(m),e&&t(R),e&&t(k),e&&t(o),e&&t(u),e&&t(w),e&&t(y),e&&t(T),e&&t(q),e&&t(M),e&&t(x),e&&t(W),e&&t(G),e&&t(P),e&&t(J),e&&t(Z),e&&t(j),e&&t(K),e&&t(L),e&&t(D),e&&t(N),e&&t(V),e&&t(X),e&&t(Y),e&&t(g),e&&t(U),e&&t(ee),e&&t(te),e&&t(ne),e&&t(ie),e&&t(re),e&&t(Q),e&&t(se),e&&t(oe),e&&t(le),e&&t(fe),e&&t(ae),e&&t(O),e&&t(ue),e&&t(pe),e&&t(be),e&&t(_e),e&&t(ce),e&&t(H),e&&t(de),e&&t(me),e&&t(Re),e&&t(he),e&&t(ke),e&&t(C),e&&t(ve),e&&t(Ee),e&&t(Te),e&&t(we),e&&t(ye),e&&t(S),e&&t(Be),e&&t($e),e&&t(Ie),e&&t(F),e&&t(ze),e&&t(Pe),e&&t(De),e&&t(B),e&&t(Ue),e&&t(Qe),e&&t(Oe),e&&t(A),e&&t(He),e&&t(Ce),e&&t(Se),e&&t($)}}}function Dt(I){let a,_,b,h,m,R,c,k;return a=new $t({props:{title:"Token Endpoint"}}),b=new ft({props:{title:"Introduction",$$slots:{default:[It]},$$scope:{ctx:I}}}),m=new ft({props:{title:"Specifications",$$slots:{default:[zt]},$$scope:{ctx:I}}}),c=new ft({props:{title:"Token",$$slots:{default:[Pt]},$$scope:{ctx:I}}}),{c(){qe(a.$$.fragment),_=v(),qe(b.$$.fragment),h=v(),qe(m.$$.fragment),R=v(),qe(c.$$.fragment)},l(o){Me(a.$$.fragment,o),_=E(o),Me(b.$$.fragment,o),h=E(o),Me(m.$$.fragment,o),R=E(o),Me(c.$$.fragment,o)},m(o,u){xe(a,o,u),n(o,_,u),xe(b,o,u),n(o,h,u),xe(m,o,u),n(o,R,u),xe(c,o,u),k=!0},p(o,[u]){const w={};u&1&&(w.$$scope={dirty:u,ctx:o}),b.$set(w);const y={};u&1&&(y.$$scope={dirty:u,ctx:o}),m.$set(y);const T={};u&1&&(T.$$scope={dirty:u,ctx:o}),c.$set(T)},i(o){k||(We(a.$$.fragment,o),We(b.$$.fragment,o),We(m.$$.fragment,o),We(c.$$.fragment,o),k=!0)},o(o){Ge(a.$$.fragment,o),Ge(b.$$.fragment,o),Ge(m.$$.fragment,o),Ge(c.$$.fragment,o),k=!1},d(o){Je(a,o),o&&t(_),Je(b,o),o&&t(h),Je(m,o),o&&t(R),Je(c,o)}}}class Ht extends wt{constructor(a){super(),yt(this,a,null,Dt,Bt,{})}}export{Ht as component};
