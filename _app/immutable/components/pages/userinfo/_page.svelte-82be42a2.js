import{S as pe,i as ae,s as ue,c as I,a as E,b as G,d as b,m as q,k as r,o as y,p as w,q as D,j as s,e as u,t as c,f as m,g as R,h as _,l as v,n as re,r as fe}from"../../../chunks/index-8ff2e5dd.js";import{D as me}from"../../../chunks/Diagram-61881b48.js";import{P as $e}from"../../../chunks/PageTitle-edc21ece.js";import{S as ne}from"../../../chunks/Section-732b932d.js";function ce(k){let i,l;return{c(){i=u("p"),l=c("An endpoint to get claims about a user.")},l(o){i=m(o,"P",{});var p=R(i);l=_(p,"An endpoint to get claims about a user."),p.forEach(s)},m(o,p){r(o,i,p),v(i,l)},p:re,d(o){o&&s(i)}}}function _e(k){let i,l,o,p;return{c(){i=u("ul"),l=u("li"),o=u("a"),p=c("OpenId Connect 1.0"),this.h()},l(f){i=m(f,"UL",{class:!0});var T=R(i);l=m(T,"LI",{});var $=R(l);o=m($,"A",{href:!0});var P=R(o);p=_(P,"OpenId Connect 1.0"),P.forEach(s),$.forEach(s),T.forEach(s),this.h()},h(){fe(o,"href","https://openid.net/specs/openid-connect-core-1_0.html#UserInfo"),fe(i,"class","list-disc")},m(f,T){r(f,i,T),v(i,l),v(l,o),v(o,p)},p:re,d(f){f&&s(i)}}}function Te(k){let i=`
sequenceDiagram
participant OpenIDProvider as OP
participant RelyingParty as RP
RelyingParty->>OpenIDProvider: Get request userinfo
OpenIDProvider->>RelyingParty: 200 response userinfo
`,l;return{c(){l=c(i)},l(o){l=_(o,i)},m(o,p){r(o,l,p)},p:re,d(o){o&&s(l)}}}function Pe(k){let i,l,o,p,f,T,$,P,t,a,A,x,S,z,j,C,K,d,Z,U,W,h,V,J,O,g,ie=`{
     "sub": "248289761001",
     "name": "Jane Doe",
     "given_name": "Jane",
     "family_name": "Doe",
     "email": "janedoe@example.com"
    }`,ee,te,L,F,M,N,Q,B,se,X;return f=new me({props:{$$slots:{default:[Te]},$$scope:{ctx:k}}}),{c(){i=c(`The endpoint accepts GET and POST methods.
It requires an Authorization header with an issued access token.
The access token must contain the `),l=u("code"),o=c("identityprovider:userinfo"),p=c(` scope.
The response is a json body of claims, which are identified
from the sub claim in the access token.
`),I(f.$$.fragment),T=E(),$=u("br"),P=c(`
Example of a GET request:`),t=u("br"),a=E(),A=u("pre"),x=c(`    GET /connect/userinfo HTTP/1.1
    Host: idp.authserver.dk
    Authorization: Bearer SlAV32hkKG
`),S=E(),z=u("br"),j=c(`
Example of a POST request:`),C=u("br"),K=E(),d=u("pre"),Z=c(`    POST /connect/userinfo HTTP/1.1
    Host: idp.authserver.dk
    Authorization: Bearer SlAV32hkKG
`),U=E(),W=u("br"),h=c(`
Example of a successful response:`),V=u("br"),J=E(),O=u("pre"),g=c(`    HTTP/1.1 200 OK
    Content-Type: application/json
  
    `),ee=c(ie),te=c(`
`),L=E(),F=u("br"),M=c(`
Example of an error response:`),N=u("br"),Q=E(),B=u("pre"),se=c(`    HTTP/1.1 401 Unauthorized
    WWW-Authenticate: error="invalid_token",
      error_description="The Access Token expired"
`)},l(e){i=_(e,`The endpoint accepts GET and POST methods.
It requires an Authorization header with an issued access token.
The access token must contain the `),l=m(e,"CODE",{});var n=R(l);o=_(n,"identityprovider:userinfo"),n.forEach(s),p=_(e,` scope.
The response is a json body of claims, which are identified
from the sub claim in the access token.
`),G(f.$$.fragment,e),T=b(e),$=m(e,"BR",{}),P=_(e,`
Example of a GET request:`),t=m(e,"BR",{}),a=b(e),A=m(e,"PRE",{});var H=R(A);x=_(H,`    GET /connect/userinfo HTTP/1.1
    Host: idp.authserver.dk
    Authorization: Bearer SlAV32hkKG
`),H.forEach(s),S=b(e),z=m(e,"BR",{}),j=_(e,`
Example of a POST request:`),C=m(e,"BR",{}),K=b(e),d=m(e,"PRE",{});var oe=R(d);Z=_(oe,`    POST /connect/userinfo HTTP/1.1
    Host: idp.authserver.dk
    Authorization: Bearer SlAV32hkKG
`),oe.forEach(s),U=b(e),W=m(e,"BR",{}),h=_(e,`
Example of a successful response:`),V=m(e,"BR",{}),J=b(e),O=m(e,"PRE",{});var Y=R(O);g=_(Y,`    HTTP/1.1 200 OK
    Content-Type: application/json
  
    `),ee=_(Y,ie),te=_(Y,`
`),Y.forEach(s),L=b(e),F=m(e,"BR",{}),M=_(e,`
Example of an error response:`),N=m(e,"BR",{}),Q=b(e),B=m(e,"PRE",{});var le=R(B);se=_(le,`    HTTP/1.1 401 Unauthorized
    WWW-Authenticate: error="invalid_token",
      error_description="The Access Token expired"
`),le.forEach(s)},m(e,n){r(e,i,n),r(e,l,n),v(l,o),r(e,p,n),q(f,e,n),r(e,T,n),r(e,$,n),r(e,P,n),r(e,t,n),r(e,a,n),r(e,A,n),v(A,x),r(e,S,n),r(e,z,n),r(e,j,n),r(e,C,n),r(e,K,n),r(e,d,n),v(d,Z),r(e,U,n),r(e,W,n),r(e,h,n),r(e,V,n),r(e,J,n),r(e,O,n),v(O,g),v(O,ee),v(O,te),r(e,L,n),r(e,F,n),r(e,M,n),r(e,N,n),r(e,Q,n),r(e,B,n),v(B,se),X=!0},p(e,n){const H={};n&1&&(H.$$scope={dirty:n,ctx:e}),f.$set(H)},i(e){X||(y(f.$$.fragment,e),X=!0)},o(e){w(f.$$.fragment,e),X=!1},d(e){e&&s(i),e&&s(l),e&&s(p),D(f,e),e&&s(T),e&&s($),e&&s(P),e&&s(t),e&&s(a),e&&s(A),e&&s(S),e&&s(z),e&&s(j),e&&s(C),e&&s(K),e&&s(d),e&&s(U),e&&s(W),e&&s(h),e&&s(V),e&&s(J),e&&s(O),e&&s(L),e&&s(F),e&&s(M),e&&s(N),e&&s(Q),e&&s(B)}}}function Ee(k){let i,l,o,p,f,T,$,P;return i=new $e({props:{title:"UserInfo Endpoint"}}),o=new ne({props:{title:"Introduction",$$slots:{default:[ce]},$$scope:{ctx:k}}}),f=new ne({props:{title:"Specifications",$$slots:{default:[_e]},$$scope:{ctx:k}}}),$=new ne({props:{title:"UserInfo",$$slots:{default:[Pe]},$$scope:{ctx:k}}}),{c(){I(i.$$.fragment),l=E(),I(o.$$.fragment),p=E(),I(f.$$.fragment),T=E(),I($.$$.fragment)},l(t){G(i.$$.fragment,t),l=b(t),G(o.$$.fragment,t),p=b(t),G(f.$$.fragment,t),T=b(t),G($.$$.fragment,t)},m(t,a){q(i,t,a),r(t,l,a),q(o,t,a),r(t,p,a),q(f,t,a),r(t,T,a),q($,t,a),P=!0},p(t,[a]){const A={};a&1&&(A.$$scope={dirty:a,ctx:t}),o.$set(A);const x={};a&1&&(x.$$scope={dirty:a,ctx:t}),f.$set(x);const S={};a&1&&(S.$$scope={dirty:a,ctx:t}),$.$set(S)},i(t){P||(y(i.$$.fragment,t),y(o.$$.fragment,t),y(f.$$.fragment,t),y($.$$.fragment,t),P=!0)},o(t){w(i.$$.fragment,t),w(o.$$.fragment,t),w(f.$$.fragment,t),w($.$$.fragment,t),P=!1},d(t){D(i,t),t&&s(l),D(o,t),t&&s(p),D(f,t),t&&s(T),D($,t)}}}class Re extends pe{constructor(i){super(),ae(this,i,null,Ee,ue,{})}}export{Re as default};
