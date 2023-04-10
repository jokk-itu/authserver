import{S as z,i as L,s as b,y as m,a as I,z as u,c as R,A as h,b as p,g as _,d as g,B as d,h as f,k,q as A,l as v,m as P,r as O,D as w,H as S,n as x}from"../chunks/index.2ef5bca6.js";import{D as B}from"../chunks/Diagram.9ecf90b9.js";import{P as C}from"../chunks/PageTitle.4f3ee9c3.js";import{S as D}from"../chunks/Section.7d0b7be1.js";function H(c){let s,n;return{c(){s=k("p"),n=A(`If a client is authorized to use the refresh token grant,
it receives a refresh token alongside its access token
in the token endpoint response.
It is used to refresh new access tokens,
since access tokens have a short lifespan.
A refresh token however, has a much longer lifespan.`)},l(e){s=v(e,"P",{});var a=P(s);n=O(a,`If a client is authorized to use the refresh token grant,
it receives a refresh token alongside its access token
in the token endpoint response.
It is used to refresh new access tokens,
since access tokens have a short lifespan.
A refresh token however, has a much longer lifespan.`),a.forEach(f)},m(e,a){p(e,s,a),w(s,n)},p:S,d(e){e&&f(s)}}}function T(c){let s,n,e,a;return{c(){s=k("ul"),n=k("li"),e=k("a"),a=A("OAuth 2.1"),this.h()},l(r){s=v(r,"UL",{class:!0});var l=P(s);n=v(l,"LI",{});var i=P(n);e=v(i,"A",{href:!0});var $=P(e);a=O($,"OAuth 2.1"),$.forEach(f),i.forEach(f),l.forEach(f),this.h()},h(){x(e,"href","https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-08"),x(s,"class","list-disc")},m(r,l){p(r,s,l),w(s,n),w(n,e),w(e,a)},p:S,d(r){r&&f(s)}}}function U(c){let s=`
sequenceDiagram
participant RelyingParty as RP
participant OpenIDProvider as OP
RelyingParty->>OpenIDProvider: Request refresh through token endpoint
OpenIDProvider->>RelyingParty: Response with access token
`,n;return{c(){n=A(s)},l(e){n=O(e,s)},m(e,a){p(e,n,a)},p:S,d(e){e&&f(n)}}}function j(c){let s,n;return s=new B({props:{$$slots:{default:[U]},$$scope:{ctx:c}}}),{c(){m(s.$$.fragment)},l(e){u(s.$$.fragment,e)},m(e,a){h(s,e,a),n=!0},p(e,a){const r={};a&1&&(r.$$scope={dirty:a,ctx:e}),s.$set(r)},i(e){n||(_(s.$$.fragment,e),n=!0)},o(e){g(s.$$.fragment,e),n=!1},d(e){d(s,e)}}}function F(c){let s,n,e,a,r,l,i,$;return s=new C({props:{title:"Refresh token"}}),e=new D({props:{title:"Introduction",$$slots:{default:[H]},$$scope:{ctx:c}}}),r=new D({props:{title:"Specifications",$$slots:{default:[T]},$$scope:{ctx:c}}}),i=new D({props:{title:"Refresh token",$$slots:{default:[j]},$$scope:{ctx:c}}}),{c(){m(s.$$.fragment),n=I(),m(e.$$.fragment),a=I(),m(r.$$.fragment),l=I(),m(i.$$.fragment)},l(t){u(s.$$.fragment,t),n=R(t),u(e.$$.fragment,t),a=R(t),u(r.$$.fragment,t),l=R(t),u(i.$$.fragment,t)},m(t,o){h(s,t,o),p(t,n,o),h(e,t,o),p(t,a,o),h(r,t,o),p(t,l,o),h(i,t,o),$=!0},p(t,[o]){const q={};o&1&&(q.$$scope={dirty:o,ctx:t}),e.$set(q);const y={};o&1&&(y.$$scope={dirty:o,ctx:t}),r.$set(y);const E={};o&1&&(E.$$scope={dirty:o,ctx:t}),i.$set(E)},i(t){$||(_(s.$$.fragment,t),_(e.$$.fragment,t),_(r.$$.fragment,t),_(i.$$.fragment,t),$=!0)},o(t){g(s.$$.fragment,t),g(e.$$.fragment,t),g(r.$$.fragment,t),g(i.$$.fragment,t),$=!1},d(t){d(s,t),t&&f(n),d(e,t),t&&f(a),d(r,t),t&&f(l),d(i,t)}}}class N extends z{constructor(s){super(),L(this,s,null,F,b,{})}}export{N as default};
