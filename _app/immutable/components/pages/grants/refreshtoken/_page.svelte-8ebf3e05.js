import{S as z,i as L,s as b,c as m,a as I,b as u,d as R,m as h,k as p,o as _,p as g,q as d,j as f,e as k,t as O,f as v,g as P,h as A,l as w,n as S,r as y}from"../../../../chunks/index-8ff2e5dd.js";import{D as j}from"../../../../chunks/Diagram-61881b48.js";import{P as C}from"../../../../chunks/PageTitle-edc21ece.js";import{S as D}from"../../../../chunks/Section-3eaacaa0.js";function T(c){let s,n;return{c(){s=k("p"),n=O(`If a client is authorized to use the refresh token grant,
it receives a refresh token alongside its access token
in the token endpoint response.
It is used to refresh new access tokens,
since access tokens have a short lifespan.
A refresh token however, has a much longer lifespan.`)},l(e){s=v(e,"P",{});var a=P(s);n=A(a,`If a client is authorized to use the refresh token grant,
it receives a refresh token alongside its access token
in the token endpoint response.
It is used to refresh new access tokens,
since access tokens have a short lifespan.
A refresh token however, has a much longer lifespan.`),a.forEach(f)},m(e,a){p(e,s,a),w(s,n)},p:S,d(e){e&&f(s)}}}function U(c){let s,n,e,a;return{c(){s=k("ul"),n=k("li"),e=k("a"),a=O("OAuth 2.1"),this.h()},l(r){s=v(r,"UL",{class:!0});var l=P(s);n=v(l,"LI",{});var i=P(n);e=v(i,"A",{href:!0});var $=P(e);a=A($,"OAuth 2.1"),$.forEach(f),i.forEach(f),l.forEach(f),this.h()},h(){y(e,"href","https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-08"),y(s,"class","list-disc")},m(r,l){p(r,s,l),w(s,n),w(n,e),w(e,a)},p:S,d(r){r&&f(s)}}}function B(c){let s=`
sequenceDiagram
participant RelyingParty as RP
participant OpenIDProvider as OP
RelyingParty->>OpenIDProvider: Request refresh through token endpoint
OpenIDProvider->>RelyingParty: Response with access token
`,n;return{c(){n=O(s)},l(e){n=A(e,s)},m(e,a){p(e,n,a)},p:S,d(e){e&&f(n)}}}function F(c){let s,n;return s=new j({props:{$$slots:{default:[B]},$$scope:{ctx:c}}}),{c(){m(s.$$.fragment)},l(e){u(s.$$.fragment,e)},m(e,a){h(s,e,a),n=!0},p(e,a){const r={};a&1&&(r.$$scope={dirty:a,ctx:e}),s.$set(r)},i(e){n||(_(s.$$.fragment,e),n=!0)},o(e){g(s.$$.fragment,e),n=!1},d(e){d(s,e)}}}function G(c){let s,n,e,a,r,l,i,$;return s=new C({props:{title:"Refresh token"}}),e=new D({props:{title:"Introduction",$$slots:{default:[T]},$$scope:{ctx:c}}}),r=new D({props:{title:"Specifications",$$slots:{default:[U]},$$scope:{ctx:c}}}),i=new D({props:{title:"Refresh token",$$slots:{default:[F]},$$scope:{ctx:c}}}),{c(){m(s.$$.fragment),n=I(),m(e.$$.fragment),a=I(),m(r.$$.fragment),l=I(),m(i.$$.fragment)},l(t){u(s.$$.fragment,t),n=R(t),u(e.$$.fragment,t),a=R(t),u(r.$$.fragment,t),l=R(t),u(i.$$.fragment,t)},m(t,o){h(s,t,o),p(t,n,o),h(e,t,o),p(t,a,o),h(r,t,o),p(t,l,o),h(i,t,o),$=!0},p(t,[o]){const q={};o&1&&(q.$$scope={dirty:o,ctx:t}),e.$set(q);const E={};o&1&&(E.$$scope={dirty:o,ctx:t}),r.$set(E);const x={};o&1&&(x.$$scope={dirty:o,ctx:t}),i.$set(x)},i(t){$||(_(s.$$.fragment,t),_(e.$$.fragment,t),_(r.$$.fragment,t),_(i.$$.fragment,t),$=!0)},o(t){g(s.$$.fragment,t),g(e.$$.fragment,t),g(r.$$.fragment,t),g(i.$$.fragment,t),$=!1},d(t){d(s,t),t&&f(n),d(e,t),t&&f(a),d(r,t),t&&f(l),d(i,t)}}}class N extends z{constructor(s){super(),L(this,s,null,G,b,{})}}export{N as default};
