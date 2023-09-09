import{S as mt,i as dt,s as Rt,c as He,a as T,b as Oe,d as E,m as Ce,k as n,o as Se,p as Fe,q as qe,j as t,t as s,h as f,e as r,f as l,g as m,r as et,l as p,n as ct}from"../../../chunks/index-8ff2e5dd.js";import{P as kt}from"../../../chunks/PageTitle-edc21ece.js";import{S as tt}from"../../../chunks/Section-732b932d.js";function ht(I){let a;return{c(){a=s("The token endpoint is used by the OP to issue access, refresh and id tokens to RPs by redeeming grants.")},l(b){a=f(b,"The token endpoint is used by the OP to issue access, refresh and id tokens to RPs by redeeming grants.")},m(b,_){n(b,a,_)},d(b){b&&t(a)}}}function vt(I){let a,b,_,k,d,R,c,h;return{c(){a=r("ul"),b=r("li"),_=r("a"),k=s("OIDC 1.0"),d=T(),R=r("li"),c=r("a"),h=s("OAuth 2.1"),this.h()},l(o){a=l(o,"UL",{class:!0});var u=m(a);b=l(u,"LI",{});var w=m(b);_=l(w,"A",{href:!0});var $=m(_);k=f($,"OIDC 1.0"),$.forEach(t),w.forEach(t),d=E(u),R=l(u,"LI",{});var v=m(R);c=l(v,"A",{href:!0});var z=m(c);h=f(z,"OAuth 2.1"),z.forEach(t),v.forEach(t),u.forEach(t),this.h()},h(){et(_,"href","https://openid.net/specs/openid-connect-core-1_0.html"),et(c,"href","https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-09"),et(a,"class","list-disc")},m(o,u){n(o,a,u),p(a,b),p(b,_),p(_,k),p(a,d),p(a,R),p(R,c),p(c,h)},p:ct,d(o){o&&t(a)}}}function Tt(I){let a,b,_,k,d,R,c,h,o,u,w,$,v,z,q,A,M,W,x,P,Ae,j,G,J,Z,K,D,Me,L,N,V,X,Y,Q,We,g,ee,te,ne,ie,U,xe,re,le,oe,se,fe,H,je,ae,ue,pe,_e,be,O,Ge,ce,me,de,Re,ke,C,Je,he,ve,Te,S,Ze,Ee,we,$e,B,Ke,nt=`{
     "access_token": "aiseubisadvsdbgur",
     "expires_in": 3600,
     "refresh_token": "lauribvidbvdfv";
     "id_token": "aleryubvksjdv",
    }`,Le,Ne,Be,ye,Ie,F,Ve,ze,Pe,De,y,Xe,it=`{
     "access_token": "aiseubisadvsdbgur",
     "expires_in": 3600
    }`,Ye,ge;return{c(){a=s(`Requests are sent using the POST method,
and the body is encoded as form-urlencoded.
The client must also authenticate itself,
using the authentication method registered in the token_endpoint_auth_method.`),b=r("br"),_=s(`
The following parameters are allowed:`),k=r("br"),d=T(),R=r("b"),c=s("grant_type"),h=r("br"),o=s(`
REQUIRED. The grant being redeemed for token(s).
The supported grants can be found at the discovery endpoint under grant_types.
`),u=r("br"),w=r("br"),$=T(),v=r("b"),z=s("code"),q=r("br"),A=s(`
REQUIRED. If the grant_type is authorization_code.
It is the authorization_code issued by the OP.
`),M=r("br"),W=r("br"),x=T(),P=r("b"),Ae=s("client_id"),j=r("br"),G=s(`
REQUIRED. Used to authenticate the client.
How it is sent is determined by the client authentication method.
`),J=r("br"),Z=r("br"),K=T(),D=r("b"),Me=s("client_secret"),L=r("br"),N=s(`
REQUIRED. Used to authenticate the client.
How it is sent is determined by the client authenticatino method.
`),V=r("br"),X=r("br"),Y=T(),Q=r("b"),We=s("redirect_uri"),g=r("br"),ee=s(`
REQUIRED. If the grant_type is authorization_code
and the authorize request included a redirect_uri parameter.
If included, it must match a registered redirect_uri,
and it must match the same redirect_uri given at the authorize endpoint, if one was provided.
`),te=r("br"),ne=r("br"),ie=T(),U=r("b"),xe=s("scope"),re=r("br"),le=s(`
REQUIRED. If the grant_type is client_credentials.
It can be useful during refresh_token grant,
if the access token should only contain a subset of the authorized scope in the initial grant.
`),oe=r("br"),se=r("br"),fe=T(),H=r("b"),je=s("code_verifier"),ae=r("br"),ue=s(`
REQUIRED. If the grant_type is authorization_code.
It will be validated against the code, since it contains the code_challenge from the authorize request.
It must be between 43 and 128 characters long.
`),pe=r("br"),_e=r("br"),be=T(),O=r("b"),Ge=s("refresh_token"),ce=r("br"),me=s(`
REQUIRED. If the grant_type is refresh_token.
It will be used and then invalidated. A new refresh_token is returned in the response.
`),de=r("br"),Re=r("br"),ke=s(`
The request for authorization_code grant can look like the following:
`),C=r("pre"),Je=s(`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=authorization_code
    &code=fgukaoirnenvsoidnv
    &redirect_uri=https://webapp.authserver.dk/signin-callback
    &code_verifier=saeoginsoivn...
`),he=r("br"),ve=r("br"),Te=s(`
The request for refresh_token grant can look like the following:
`),S=r("pre"),Ze=s(`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=refresh_token
    &refresh_token=fgukaoirnenvsoidnv
    &scope=openid%20identityprovider:userinfo
`),Ee=r("br"),we=r("br"),$e=s(`
The response for authorization_code and refresh_token grant can look like the following:
`),B=r("pre"),Ke=s(`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),Le=s(nt),Ne=s(`
`),Be=r("br"),ye=r("br"),Ie=s(`
The request for client_credentials grant can look like the following:
`),F=r("pre"),Ve=s(`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=client_credentials&scope=weather%3Aread
`),ze=r("br"),Pe=r("br"),De=s(`
The response for client_credentials grant can look like the following:
`),y=r("pre"),Xe=s(`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),Ye=s(it),ge=s(`
`)},l(e){a=f(e,`Requests are sent using the POST method,
and the body is encoded as form-urlencoded.
The client must also authenticate itself,
using the authentication method registered in the token_endpoint_auth_method.`),b=l(e,"BR",{}),_=f(e,`
The following parameters are allowed:`),k=l(e,"BR",{}),d=E(e),R=l(e,"B",{});var i=m(R);c=f(i,"grant_type"),i.forEach(t),h=l(e,"BR",{}),o=f(e,`
REQUIRED. The grant being redeemed for token(s).
The supported grants can be found at the discovery endpoint under grant_types.
`),u=l(e,"BR",{}),w=l(e,"BR",{}),$=E(e),v=l(e,"B",{});var rt=m(v);z=f(rt,"code"),rt.forEach(t),q=l(e,"BR",{}),A=f(e,`
REQUIRED. If the grant_type is authorization_code.
It is the authorization_code issued by the OP.
`),M=l(e,"BR",{}),W=l(e,"BR",{}),x=E(e),P=l(e,"B",{});var lt=m(P);Ae=f(lt,"client_id"),lt.forEach(t),j=l(e,"BR",{}),G=f(e,`
REQUIRED. Used to authenticate the client.
How it is sent is determined by the client authentication method.
`),J=l(e,"BR",{}),Z=l(e,"BR",{}),K=E(e),D=l(e,"B",{});var ot=m(D);Me=f(ot,"client_secret"),ot.forEach(t),L=l(e,"BR",{}),N=f(e,`
REQUIRED. Used to authenticate the client.
How it is sent is determined by the client authenticatino method.
`),V=l(e,"BR",{}),X=l(e,"BR",{}),Y=E(e),Q=l(e,"B",{});var st=m(Q);We=f(st,"redirect_uri"),st.forEach(t),g=l(e,"BR",{}),ee=f(e,`
REQUIRED. If the grant_type is authorization_code
and the authorize request included a redirect_uri parameter.
If included, it must match a registered redirect_uri,
and it must match the same redirect_uri given at the authorize endpoint, if one was provided.
`),te=l(e,"BR",{}),ne=l(e,"BR",{}),ie=E(e),U=l(e,"B",{});var ft=m(U);xe=f(ft,"scope"),ft.forEach(t),re=l(e,"BR",{}),le=f(e,`
REQUIRED. If the grant_type is client_credentials.
It can be useful during refresh_token grant,
if the access token should only contain a subset of the authorized scope in the initial grant.
`),oe=l(e,"BR",{}),se=l(e,"BR",{}),fe=E(e),H=l(e,"B",{});var at=m(H);je=f(at,"code_verifier"),at.forEach(t),ae=l(e,"BR",{}),ue=f(e,`
REQUIRED. If the grant_type is authorization_code.
It will be validated against the code, since it contains the code_challenge from the authorize request.
It must be between 43 and 128 characters long.
`),pe=l(e,"BR",{}),_e=l(e,"BR",{}),be=E(e),O=l(e,"B",{});var ut=m(O);Ge=f(ut,"refresh_token"),ut.forEach(t),ce=l(e,"BR",{}),me=f(e,`
REQUIRED. If the grant_type is refresh_token.
It will be used and then invalidated. A new refresh_token is returned in the response.
`),de=l(e,"BR",{}),Re=l(e,"BR",{}),ke=f(e,`
The request for authorization_code grant can look like the following:
`),C=l(e,"PRE",{});var pt=m(C);Je=f(pt,`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=authorization_code
    &code=fgukaoirnenvsoidnv
    &redirect_uri=https://webapp.authserver.dk/signin-callback
    &code_verifier=saeoginsoivn...
`),pt.forEach(t),he=l(e,"BR",{}),ve=l(e,"BR",{}),Te=f(e,`
The request for refresh_token grant can look like the following:
`),S=l(e,"PRE",{});var _t=m(S);Ze=f(_t,`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=refresh_token
    &refresh_token=fgukaoirnenvsoidnv
    &scope=openid%20identityprovider:userinfo
`),_t.forEach(t),Ee=l(e,"BR",{}),we=l(e,"BR",{}),$e=f(e,`
The response for authorization_code and refresh_token grant can look like the following:
`),B=l(e,"PRE",{});var Qe=m(B);Ke=f(Qe,`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),Le=f(Qe,nt),Ne=f(Qe,`
`),Qe.forEach(t),Be=l(e,"BR",{}),ye=l(e,"BR",{}),Ie=f(e,`
The request for client_credentials grant can look like the following:
`),F=l(e,"PRE",{});var bt=m(F);Ve=f(bt,`    POST /connect/token HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    grant_type=client_credentials&scope=weather%3Aread
`),bt.forEach(t),ze=l(e,"BR",{}),Pe=l(e,"BR",{}),De=f(e,`
The response for client_credentials grant can look like the following:
`),y=l(e,"PRE",{});var Ue=m(y);Xe=f(Ue,`    HTTP/1.1 200 OK
    Content-Type: application/json

    `),Ye=f(Ue,it),ge=f(Ue,`
`),Ue.forEach(t)},m(e,i){n(e,a,i),n(e,b,i),n(e,_,i),n(e,k,i),n(e,d,i),n(e,R,i),p(R,c),n(e,h,i),n(e,o,i),n(e,u,i),n(e,w,i),n(e,$,i),n(e,v,i),p(v,z),n(e,q,i),n(e,A,i),n(e,M,i),n(e,W,i),n(e,x,i),n(e,P,i),p(P,Ae),n(e,j,i),n(e,G,i),n(e,J,i),n(e,Z,i),n(e,K,i),n(e,D,i),p(D,Me),n(e,L,i),n(e,N,i),n(e,V,i),n(e,X,i),n(e,Y,i),n(e,Q,i),p(Q,We),n(e,g,i),n(e,ee,i),n(e,te,i),n(e,ne,i),n(e,ie,i),n(e,U,i),p(U,xe),n(e,re,i),n(e,le,i),n(e,oe,i),n(e,se,i),n(e,fe,i),n(e,H,i),p(H,je),n(e,ae,i),n(e,ue,i),n(e,pe,i),n(e,_e,i),n(e,be,i),n(e,O,i),p(O,Ge),n(e,ce,i),n(e,me,i),n(e,de,i),n(e,Re,i),n(e,ke,i),n(e,C,i),p(C,Je),n(e,he,i),n(e,ve,i),n(e,Te,i),n(e,S,i),p(S,Ze),n(e,Ee,i),n(e,we,i),n(e,$e,i),n(e,B,i),p(B,Ke),p(B,Le),p(B,Ne),n(e,Be,i),n(e,ye,i),n(e,Ie,i),n(e,F,i),p(F,Ve),n(e,ze,i),n(e,Pe,i),n(e,De,i),n(e,y,i),p(y,Xe),p(y,Ye),p(y,ge)},p:ct,d(e){e&&t(a),e&&t(b),e&&t(_),e&&t(k),e&&t(d),e&&t(R),e&&t(h),e&&t(o),e&&t(u),e&&t(w),e&&t($),e&&t(v),e&&t(q),e&&t(A),e&&t(M),e&&t(W),e&&t(x),e&&t(P),e&&t(j),e&&t(G),e&&t(J),e&&t(Z),e&&t(K),e&&t(D),e&&t(L),e&&t(N),e&&t(V),e&&t(X),e&&t(Y),e&&t(Q),e&&t(g),e&&t(ee),e&&t(te),e&&t(ne),e&&t(ie),e&&t(U),e&&t(re),e&&t(le),e&&t(oe),e&&t(se),e&&t(fe),e&&t(H),e&&t(ae),e&&t(ue),e&&t(pe),e&&t(_e),e&&t(be),e&&t(O),e&&t(ce),e&&t(me),e&&t(de),e&&t(Re),e&&t(ke),e&&t(C),e&&t(he),e&&t(ve),e&&t(Te),e&&t(S),e&&t(Ee),e&&t(we),e&&t($e),e&&t(B),e&&t(Be),e&&t(ye),e&&t(Ie),e&&t(F),e&&t(ze),e&&t(Pe),e&&t(De),e&&t(y)}}}function Et(I){let a,b,_,k,d,R,c,h;return a=new kt({props:{title:"Token Endpoint"}}),_=new tt({props:{title:"Introduction",$$slots:{default:[ht]},$$scope:{ctx:I}}}),d=new tt({props:{title:"Specifications",$$slots:{default:[vt]},$$scope:{ctx:I}}}),c=new tt({props:{title:"Token",$$slots:{default:[Tt]},$$scope:{ctx:I}}}),{c(){He(a.$$.fragment),b=T(),He(_.$$.fragment),k=T(),He(d.$$.fragment),R=T(),He(c.$$.fragment)},l(o){Oe(a.$$.fragment,o),b=E(o),Oe(_.$$.fragment,o),k=E(o),Oe(d.$$.fragment,o),R=E(o),Oe(c.$$.fragment,o)},m(o,u){Ce(a,o,u),n(o,b,u),Ce(_,o,u),n(o,k,u),Ce(d,o,u),n(o,R,u),Ce(c,o,u),h=!0},p(o,[u]){const w={};u&1&&(w.$$scope={dirty:u,ctx:o}),_.$set(w);const $={};u&1&&($.$$scope={dirty:u,ctx:o}),d.$set($);const v={};u&1&&(v.$$scope={dirty:u,ctx:o}),c.$set(v)},i(o){h||(Se(a.$$.fragment,o),Se(_.$$.fragment,o),Se(d.$$.fragment,o),Se(c.$$.fragment,o),h=!0)},o(o){Fe(a.$$.fragment,o),Fe(_.$$.fragment,o),Fe(d.$$.fragment,o),Fe(c.$$.fragment,o),h=!1},d(o){qe(a,o),o&&t(b),qe(_,o),o&&t(k),qe(d,o),o&&t(R),qe(c,o)}}}class yt extends mt{constructor(a){super(),dt(this,a,null,Et,Rt,{})}}export{yt as default};
