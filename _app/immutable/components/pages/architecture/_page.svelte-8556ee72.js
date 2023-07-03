import{S as W,i as j,s as F,c as R,a as P,b as q,d as E,m as D,k as l,o as L,p as S,q as k,j as s,e as h,t as $,f as v,g as I,h as g,l as m,n as y}from"../../../chunks/index-8ff2e5dd.js";import{P as G}from"../../../chunks/PageTitle-edc21ece.js";import{S as U}from"../../../chunks/Section-3eaacaa0.js";import{D as J}from"../../../chunks/Diagram-0f965d9f.js";function K(b){let t,o;return{c(){t=h("p"),o=$(`The AuthorizationServer is built using Razor pages for UI,
supporting a MVVM architecture and a Vertical slice architecture
for endpoints with mediator.`)},l(n){t=v(n,"P",{});var p=I(t);o=g(p,`The AuthorizationServer is built using Razor pages for UI,
supporting a MVVM architecture and a Vertical slice architecture
for endpoints with mediator.`),p.forEach(s)},m(n,p){l(n,t,p),m(t,o)},p:y,d(n){n&&s(t)}}}function N(b){let t,o,n,p,f,u,c,_,e,r,T,d,V,w,M,C,A,x;return{c(){t=h("p"),o=$(`The API consists of endpoints, where each endpoint builds the received contract,
    using the `),n=h("code"),p=$("IContextAccessor"),f=$(`. The query or command send using Mediator,
    is built and then a `),u=h("code"),c=$("Response"),_=$(` is received.
    Mediator consists of a pipeline including a Validator pipe and a Logging pipe.`),e=P(),r=h("br"),T=P(),d=h("p"),V=$(`The Validator pipe is responsible for invoking all registered validators for the query or command.
    The Logging pipe is responsible for logging.`),w=P(),M=h("br"),C=P(),A=h("p"),x=$(`When the response is received, it is checked for errors,
    and an appropiate HTTP response is finally returned.`)},l(i){t=v(i,"P",{});var a=I(t);o=g(a,`The API consists of endpoints, where each endpoint builds the received contract,
    using the `),n=v(a,"CODE",{});var z=I(n);p=g(z,"IContextAccessor"),z.forEach(s),f=g(a,`. The query or command send using Mediator,
    is built and then a `),u=v(a,"CODE",{});var B=I(u);c=g(B,"Response"),B.forEach(s),_=g(a,` is received.
    Mediator consists of a pipeline including a Validator pipe and a Logging pipe.`),a.forEach(s),e=E(i),r=v(i,"BR",{}),T=E(i),d=v(i,"P",{});var H=I(d);V=g(H,`The Validator pipe is responsible for invoking all registered validators for the query or command.
    The Logging pipe is responsible for logging.`),H.forEach(s),w=E(i),M=v(i,"BR",{}),C=E(i),A=v(i,"P",{});var O=I(A);x=g(O,`When the response is received, it is checked for errors,
    and an appropiate HTTP response is finally returned.`),O.forEach(s)},m(i,a){l(i,t,a),m(t,o),m(t,n),m(n,p),m(t,f),m(t,u),m(u,c),m(t,_),l(i,e,a),l(i,r,a),l(i,T,a),l(i,d,a),m(d,V),l(i,w,a),l(i,M,a),l(i,C,a),l(i,A,a),m(A,x)},p:y,d(i){i&&s(t),i&&s(e),i&&s(r),i&&s(T),i&&s(d),i&&s(w),i&&s(M),i&&s(C),i&&s(A)}}}function Q(b){let t=`
        flowchart LR
        A(IEndpoint)
        B(IMediator)
        C(ValidatorPipeline)
        D(LoggingPipeline)
        E(Handler)
        A --> B --> C --> D --> E
    `,o;return{c(){o=$(t)},l(n){o=g(n,t)},m(n,p){l(n,o,p)},p:y,d(n){n&&s(o)}}}function X(b){let t,o,n,p,f,u,c,_;return t=new G({props:{title:"Architecture"}}),n=new U({props:{title:"Introduction",$$slots:{default:[K]},$$scope:{ctx:b}}}),f=new U({props:{title:"API",$$slots:{default:[N]},$$scope:{ctx:b}}}),c=new J({props:{$$slots:{default:[Q]},$$scope:{ctx:b}}}),{c(){R(t.$$.fragment),o=P(),R(n.$$.fragment),p=P(),R(f.$$.fragment),u=P(),R(c.$$.fragment)},l(e){q(t.$$.fragment,e),o=E(e),q(n.$$.fragment,e),p=E(e),q(f.$$.fragment,e),u=E(e),q(c.$$.fragment,e)},m(e,r){D(t,e,r),l(e,o,r),D(n,e,r),l(e,p,r),D(f,e,r),l(e,u,r),D(c,e,r),_=!0},p(e,[r]){const T={};r&1&&(T.$$scope={dirty:r,ctx:e}),n.$set(T);const d={};r&1&&(d.$$scope={dirty:r,ctx:e}),f.$set(d);const V={};r&1&&(V.$$scope={dirty:r,ctx:e}),c.$set(V)},i(e){_||(L(t.$$.fragment,e),L(n.$$.fragment,e),L(f.$$.fragment,e),L(c.$$.fragment,e),_=!0)},o(e){S(t.$$.fragment,e),S(n.$$.fragment,e),S(f.$$.fragment,e),S(c.$$.fragment,e),_=!1},d(e){k(t,e),e&&s(o),k(n,e),e&&s(p),k(f,e),e&&s(u),k(c,e)}}}class ie extends W{constructor(t){super(),j(this,t,null,X,F,{})}}export{ie as default};
