import{S as pe,i as ae,s as ue,y as I,a as E,z as y,c as b,A as G,b as r,g as q,d as D,B as w,h as n,k as u,q as c,l as m,m as R,r as _,D as v,H as re,n as fe}from"../chunks/index.8cffed02.js";import{D as me}from"../chunks/Diagram.ad75e5b6.js";import{P as $e}from"../chunks/PageTitle.df959b80.js";import{S as se}from"../chunks/Section.be805b22.js";function ce(k){let i,l;return{c(){i=u("p"),l=c("An endpoint to get claims about a user.")},l(o){i=m(o,"P",{});var p=R(i);l=_(p,"An endpoint to get claims about a user."),p.forEach(n)},m(o,p){r(o,i,p),v(i,l)},p:re,d(o){o&&n(i)}}}function _e(k){let i,l,o,p;return{c(){i=u("ul"),l=u("li"),o=u("a"),p=c("OpenId Connect 1.0"),this.h()},l(f){i=m(f,"UL",{class:!0});var T=R(i);l=m(T,"LI",{});var $=R(l);o=m($,"A",{href:!0});var P=R(o);p=_(P,"OpenId Connect 1.0"),P.forEach(n),$.forEach(n),T.forEach(n),this.h()},h(){fe(o,"href","https://openid.net/specs/openid-connect-core-1_0.html#UserInfo"),fe(i,"class","list-disc")},m(f,T){r(f,i,T),v(i,l),v(l,o),v(o,p)},p:re,d(f){f&&n(i)}}}function Te(k){let i=`
sequenceDiagram
participant OpenIDProvider as OP
participant RelyingParty as RP
RelyingParty->>OpenIDProvider: Get request userinfo
OpenIDProvider->>RelyingParty: 200 response userinfo
`,l;return{c(){l=c(i)},l(o){l=_(o,i)},m(o,p){r(o,l,p)},p:re,d(o){o&&n(l)}}}function Pe(k){let i,l,o,p,f,T,$,P,t,a,A,x,S,z,C,K,U,B,Z,W,j,h,V,J,O,g,ie=`{
     "sub": "248289761001",
     "name": "Jane Doe",
     "given_name": "Jane",
     "family_name": "Doe",
     "email": "janedoe@example.com"
    }`,ee,te,L,F,M,N,Q,H,ne,X;return f=new me({props:{$$slots:{default:[Te]},$$scope:{ctx:k}}}),{c(){i=c(`The endpoint accepts GET and POST methods.
It requires an Authorization header with an issued access token.
The access token must contain the `),l=u("code"),o=c("identityprovider:userinfo"),p=c(` scope.
The response is a json body of claims, which are identified
from the sub claim in the access token.
`),I(f.$$.fragment),T=E(),$=u("br"),P=c(`
Example of a GET request:`),t=u("br"),a=E(),A=u("pre"),x=c(`    GET /connect/userinfo HTTP/1.1
    Host: idp.authserver.dk
    Authorization: Bearer SlAV32hkKG
`),S=E(),z=u("br"),C=c(`
Example of a POST request:`),K=u("br"),U=E(),B=u("pre"),Z=c(`    POST /connect/userinfo HTTP/1.1
    Host: idp.authserver.dk
    Authorization: Bearer SlAV32hkKG
`),W=E(),j=u("br"),h=c(`
Example of a successful response:`),V=u("br"),J=E(),O=u("pre"),g=c(`    HTTP/1.1 200 OK
    Content-Type: application/json
  
    `),ee=c(ie),te=c(`
`),L=E(),F=u("br"),M=c(`
Example of an error response:`),N=u("br"),Q=E(),H=u("pre"),ne=c(`    HTTP/1.1 401 Unauthorized
    WWW-Authenticate: error="invalid_token",
      error_description="The Access Token expired"
`)},l(e){i=_(e,`The endpoint accepts GET and POST methods.
It requires an Authorization header with an issued access token.
The access token must contain the `),l=m(e,"CODE",{});var s=R(l);o=_(s,"identityprovider:userinfo"),s.forEach(n),p=_(e,` scope.
The response is a json body of claims, which are identified
from the sub claim in the access token.
`),y(f.$$.fragment,e),T=b(e),$=m(e,"BR",{}),P=_(e,`
Example of a GET request:`),t=m(e,"BR",{}),a=b(e),A=m(e,"PRE",{});var d=R(A);x=_(d,`    GET /connect/userinfo HTTP/1.1
    Host: idp.authserver.dk
    Authorization: Bearer SlAV32hkKG
`),d.forEach(n),S=b(e),z=m(e,"BR",{}),C=_(e,`
Example of a POST request:`),K=m(e,"BR",{}),U=b(e),B=m(e,"PRE",{});var oe=R(B);Z=_(oe,`    POST /connect/userinfo HTTP/1.1
    Host: idp.authserver.dk
    Authorization: Bearer SlAV32hkKG
`),oe.forEach(n),W=b(e),j=m(e,"BR",{}),h=_(e,`
Example of a successful response:`),V=m(e,"BR",{}),J=b(e),O=m(e,"PRE",{});var Y=R(O);g=_(Y,`    HTTP/1.1 200 OK
    Content-Type: application/json
  
    `),ee=_(Y,ie),te=_(Y,`
`),Y.forEach(n),L=b(e),F=m(e,"BR",{}),M=_(e,`
Example of an error response:`),N=m(e,"BR",{}),Q=b(e),H=m(e,"PRE",{});var le=R(H);ne=_(le,`    HTTP/1.1 401 Unauthorized
    WWW-Authenticate: error="invalid_token",
      error_description="The Access Token expired"
`),le.forEach(n)},m(e,s){r(e,i,s),r(e,l,s),v(l,o),r(e,p,s),G(f,e,s),r(e,T,s),r(e,$,s),r(e,P,s),r(e,t,s),r(e,a,s),r(e,A,s),v(A,x),r(e,S,s),r(e,z,s),r(e,C,s),r(e,K,s),r(e,U,s),r(e,B,s),v(B,Z),r(e,W,s),r(e,j,s),r(e,h,s),r(e,V,s),r(e,J,s),r(e,O,s),v(O,g),v(O,ee),v(O,te),r(e,L,s),r(e,F,s),r(e,M,s),r(e,N,s),r(e,Q,s),r(e,H,s),v(H,ne),X=!0},p(e,s){const d={};s&1&&(d.$$scope={dirty:s,ctx:e}),f.$set(d)},i(e){X||(q(f.$$.fragment,e),X=!0)},o(e){D(f.$$.fragment,e),X=!1},d(e){e&&n(i),e&&n(l),e&&n(p),w(f,e),e&&n(T),e&&n($),e&&n(P),e&&n(t),e&&n(a),e&&n(A),e&&n(S),e&&n(z),e&&n(C),e&&n(K),e&&n(U),e&&n(B),e&&n(W),e&&n(j),e&&n(h),e&&n(V),e&&n(J),e&&n(O),e&&n(L),e&&n(F),e&&n(M),e&&n(N),e&&n(Q),e&&n(H)}}}function Ee(k){let i,l,o,p,f,T,$,P;return i=new $e({props:{title:"UserInfo Endpoint"}}),o=new se({props:{title:"Introduction",$$slots:{default:[ce]},$$scope:{ctx:k}}}),f=new se({props:{title:"Specifications",$$slots:{default:[_e]},$$scope:{ctx:k}}}),$=new se({props:{title:"UserInfo",$$slots:{default:[Pe]},$$scope:{ctx:k}}}),{c(){I(i.$$.fragment),l=E(),I(o.$$.fragment),p=E(),I(f.$$.fragment),T=E(),I($.$$.fragment)},l(t){y(i.$$.fragment,t),l=b(t),y(o.$$.fragment,t),p=b(t),y(f.$$.fragment,t),T=b(t),y($.$$.fragment,t)},m(t,a){G(i,t,a),r(t,l,a),G(o,t,a),r(t,p,a),G(f,t,a),r(t,T,a),G($,t,a),P=!0},p(t,[a]){const A={};a&1&&(A.$$scope={dirty:a,ctx:t}),o.$set(A);const x={};a&1&&(x.$$scope={dirty:a,ctx:t}),f.$set(x);const S={};a&1&&(S.$$scope={dirty:a,ctx:t}),$.$set(S)},i(t){P||(q(i.$$.fragment,t),q(o.$$.fragment,t),q(f.$$.fragment,t),q($.$$.fragment,t),P=!0)},o(t){D(i.$$.fragment,t),D(o.$$.fragment,t),D(f.$$.fragment,t),D($.$$.fragment,t),P=!1},d(t){w(i,t),t&&n(l),w(o,t),t&&n(p),w(f,t),t&&n(T),w($,t)}}}class Re extends pe{constructor(i){super(),ae(this,i,null,Ee,ue,{})}}export{Re as component};
