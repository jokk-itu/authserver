import{S as H,i as U,s as G,c as b,a as C,b as P,d as V,m as E,k as l,o as I,p as q,q as w,j as a,e as A,t as c,f as S,g as z,h as u,l as k,n as D}from"../../../chunks/index-8ff2e5dd.js";import{P as W}from"../../../chunks/PageTitle-edc21ece.js";import{S as M}from"../../../chunks/Section-732b932d.js";import{D as j}from"../../../chunks/Diagram-adaf7575.js";function K(m){let t,i;return{c(){t=A("p"),i=c(`The AuthorizationServer is built using Razor pages for UI,
supporting a MVVM architecture and a Vertical slice architecture
for endpoints with mediator.`)},l(s){t=S(s,"P",{});var f=z(t);i=u(f,`The AuthorizationServer is built using Razor pages for UI,
supporting a MVVM architecture and a Vertical slice architecture
for endpoints with mediator.`),f.forEach(a)},m(s,f){l(s,t,f),k(t,i)},p:D,d(s){s&&a(t)}}}function F(m){let t,i,s,f,r,$,d,h,g,T,_;return{c(){t=c(`The API consists of endpoints, where each endpoint builds the received contract,
using the `),i=A("code"),s=c("IContextAccessor"),f=c(`. The query or command send using Mediator,
is built and then a `),r=A("code"),$=c("Response"),d=c(` is received.
Mediator consists of a pipeline including a Validator pipe and a Logging pipe.
`),h=A("br"),g=c(`
The Validator pipe is responsible for invoking all registered validators for the query or command.
The Logging pipe is responsible for logging.
`),T=A("br"),_=c(`
When the response is received, it is checked for errors,
and an appropiate HTTP response is finally returned.`)},l(n){t=u(n,`The API consists of endpoints, where each endpoint builds the received contract,
using the `),i=S(n,"CODE",{});var p=z(i);s=u(p,"IContextAccessor"),p.forEach(a),f=u(n,`. The query or command send using Mediator,
is built and then a `),r=S(n,"CODE",{});var v=z(r);$=u(v,"Response"),v.forEach(a),d=u(n,` is received.
Mediator consists of a pipeline including a Validator pipe and a Logging pipe.
`),h=S(n,"BR",{}),g=u(n,`
The Validator pipe is responsible for invoking all registered validators for the query or command.
The Logging pipe is responsible for logging.
`),T=S(n,"BR",{}),_=u(n,`
When the response is received, it is checked for errors,
and an appropiate HTTP response is finally returned.`)},m(n,p){l(n,t,p),l(n,i,p),k(i,s),l(n,f,p),l(n,r,p),k(r,$),l(n,d,p),l(n,h,p),l(n,g,p),l(n,T,p),l(n,_,p)},p:D,d(n){n&&a(t),n&&a(i),n&&a(f),n&&a(r),n&&a(d),n&&a(h),n&&a(g),n&&a(T),n&&a(_)}}}function J(m){let t=`
        flowchart LR
        A(IEndpoint)
        B(IMediator)
        C(ValidatorPipeline)
        D(LoggingPipeline)
        E(Handler)
        A --> B --> C --> D --> E
    `,i;return{c(){i=c(t)},l(s){i=u(s,t)},m(s,f){l(s,i,f)},p:D,d(s){s&&a(i)}}}function N(m){let t;return{c(){t=c(`This is required during authorize grants, also for confidential clients.
code_challenge method must not be plain text.`)},l(i){t=u(i,`This is required during authorize grants, also for confidential clients.
code_challenge method must not be plain text.`)},m(i,s){l(i,t,s)},d(i){i&&a(t)}}}function Q(m){let t;return{c(){t=c(`Secret must be hashed at the OP and only the hash is stored.
Therefore, the secret is not returned during dynamic client registration GET requests.`)},l(i){t=u(i,`Secret must be hashed at the OP and only the hash is stored.
Therefore, the secret is not returned during dynamic client registration GET requests.`)},m(i,s){l(i,t,s)},d(i){i&&a(t)}}}function X(m){let t,i,s,f;return{c(){t=c("Scope "),i=A("code"),s=c("identityprovider:userinfo"),f=c(` is required.
It is to limit the authorization of the access token.`)},l(r){t=u(r,"Scope "),i=S(r,"CODE",{});var $=z(i);s=u($,"identityprovider:userinfo"),$.forEach(a),f=u(r,` is required.
It is to limit the authorization of the access token.`)},m(r,$){l(r,t,$),l(r,i,$),k(i,s),l(r,f,$)},p:D,d(r){r&&a(t),r&&a(i),r&&a(f)}}}function Y(m){let t,i,s,f,r,$,d,h,g,T,_,n,p,v;return t=new W({props:{title:"Architecture"}}),s=new M({props:{title:"Introduction",$$slots:{default:[K]},$$scope:{ctx:m}}}),r=new M({props:{title:"API",$$slots:{default:[F]},$$scope:{ctx:m}}}),d=new j({props:{$$slots:{default:[J]},$$scope:{ctx:m}}}),g=new M({props:{title:"PKCE",$$slots:{default:[N]},$$scope:{ctx:m}}}),_=new M({props:{title:"Client Configuration",$$slots:{default:[Q]},$$scope:{ctx:m}}}),p=new M({props:{title:"UserInfo",$$slots:{default:[X]},$$scope:{ctx:m}}}),{c(){b(t.$$.fragment),i=C(),b(s.$$.fragment),f=C(),b(r.$$.fragment),$=C(),b(d.$$.fragment),h=C(),b(g.$$.fragment),T=C(),b(_.$$.fragment),n=C(),b(p.$$.fragment)},l(e){P(t.$$.fragment,e),i=V(e),P(s.$$.fragment,e),f=V(e),P(r.$$.fragment,e),$=V(e),P(d.$$.fragment,e),h=V(e),P(g.$$.fragment,e),T=V(e),P(_.$$.fragment,e),n=V(e),P(p.$$.fragment,e)},m(e,o){E(t,e,o),l(e,i,o),E(s,e,o),l(e,f,o),E(r,e,o),l(e,$,o),E(d,e,o),l(e,h,o),E(g,e,o),l(e,T,o),E(_,e,o),l(e,n,o),E(p,e,o),v=!0},p(e,[o]){const R={};o&1&&(R.$$scope={dirty:o,ctx:e}),s.$set(R);const L={};o&1&&(L.$$scope={dirty:o,ctx:e}),r.$set(L);const O={};o&1&&(O.$$scope={dirty:o,ctx:e}),d.$set(O);const x={};o&1&&(x.$$scope={dirty:o,ctx:e}),g.$set(x);const y={};o&1&&(y.$$scope={dirty:o,ctx:e}),_.$set(y);const B={};o&1&&(B.$$scope={dirty:o,ctx:e}),p.$set(B)},i(e){v||(I(t.$$.fragment,e),I(s.$$.fragment,e),I(r.$$.fragment,e),I(d.$$.fragment,e),I(g.$$.fragment,e),I(_.$$.fragment,e),I(p.$$.fragment,e),v=!0)},o(e){q(t.$$.fragment,e),q(s.$$.fragment,e),q(r.$$.fragment,e),q(d.$$.fragment,e),q(g.$$.fragment,e),q(_.$$.fragment,e),q(p.$$.fragment,e),v=!1},d(e){w(t,e),e&&a(i),w(s,e),e&&a(f),w(r,e),e&&a($),w(d,e),e&&a(h),w(g,e),e&&a(T),w(_,e),e&&a(n),w(p,e)}}}class ne extends H{constructor(t){super(),U(this,t,null,Y,G,{})}}export{ne as default};
