import{S as z,i as C,s as L,x as m,a as I,y as u,c as R,z as h,b as p,f as _,t as g,A as d,h as f,k,q as D,l as v,m as P,r as O,E as w,C as E,n as y}from"../../../../chunks/index-86aa8d89.js";import{D as b}from"../../../../chunks/Diagram-a4d00bce.js";import{P as T}from"../../../../chunks/PageTitle-cc378c43.js";import{S as A}from"../../../../chunks/Section-4061cb73.js";function U(c){let s,n;return{c(){s=k("p"),n=D(`If a client is authorized to use the refresh token grant,
it receives a refresh token alongside its access token
in the token endpoint response.
It is used to refresh new access tokens,
since access tokens have a short lifespan.
A refresh token however, has a much longer lifespan.`)},l(e){s=v(e,"P",{});var a=P(s);n=O(a,`If a client is authorized to use the refresh token grant,
it receives a refresh token alongside its access token
in the token endpoint response.
It is used to refresh new access tokens,
since access tokens have a short lifespan.
A refresh token however, has a much longer lifespan.`),a.forEach(f)},m(e,a){p(e,s,a),w(s,n)},p:E,d(e){e&&f(s)}}}function j(c){let s,n,e,a;return{c(){s=k("ul"),n=k("li"),e=k("a"),a=D("OAuth 2.1"),this.h()},l(r){s=v(r,"UL",{class:!0});var l=P(s);n=v(l,"LI",{});var i=P(n);e=v(i,"A",{href:!0});var $=P(e);a=O($,"OAuth 2.1"),$.forEach(f),i.forEach(f),l.forEach(f),this.h()},h(){y(e,"href","https://datatracker.ietf.org/doc/html/draft-ietf-oauth-v2-1-08"),y(s,"class","list-disc")},m(r,l){p(r,s,l),w(s,n),w(n,e),w(e,a)},p:E,d(r){r&&f(s)}}}function B(c){let s=`
sequenceDiagram
participant RelyingParty as RP
participant OpenIDProvider as OP
RelyingParty->>OpenIDProvider: Request refresh through token endpoint
OpenIDProvider->>RelyingParty: Response with access token
`,n;return{c(){n=D(s)},l(e){n=O(e,s)},m(e,a){p(e,n,a)},p:E,d(e){e&&f(n)}}}function F(c){let s,n;return s=new b({props:{$$slots:{default:[B]},$$scope:{ctx:c}}}),{c(){m(s.$$.fragment)},l(e){u(s.$$.fragment,e)},m(e,a){h(s,e,a),n=!0},p(e,a){const r={};a&1&&(r.$$scope={dirty:a,ctx:e}),s.$set(r)},i(e){n||(_(s.$$.fragment,e),n=!0)},o(e){g(s.$$.fragment,e),n=!1},d(e){d(s,e)}}}function G(c){let s,n,e,a,r,l,i,$;return s=new T({props:{title:"Refresh token"}}),e=new A({props:{title:"Introduction",$$slots:{default:[U]},$$scope:{ctx:c}}}),r=new A({props:{title:"Specifications",$$slots:{default:[j]},$$scope:{ctx:c}}}),i=new A({props:{title:"Refresh token",$$slots:{default:[F]},$$scope:{ctx:c}}}),{c(){m(s.$$.fragment),n=I(),m(e.$$.fragment),a=I(),m(r.$$.fragment),l=I(),m(i.$$.fragment)},l(t){u(s.$$.fragment,t),n=R(t),u(e.$$.fragment,t),a=R(t),u(r.$$.fragment,t),l=R(t),u(i.$$.fragment,t)},m(t,o){h(s,t,o),p(t,n,o),h(e,t,o),p(t,a,o),h(r,t,o),p(t,l,o),h(i,t,o),$=!0},p(t,[o]){const S={};o&1&&(S.$$scope={dirty:o,ctx:t}),e.$set(S);const q={};o&1&&(q.$$scope={dirty:o,ctx:t}),r.$set(q);const x={};o&1&&(x.$$scope={dirty:o,ctx:t}),i.$set(x)},i(t){$||(_(s.$$.fragment,t),_(e.$$.fragment,t),_(r.$$.fragment,t),_(i.$$.fragment,t),$=!0)},o(t){g(s.$$.fragment,t),g(e.$$.fragment,t),g(r.$$.fragment,t),g(i.$$.fragment,t),$=!1},d(t){d(s,t),t&&f(n),d(e,t),t&&f(a),d(r,t),t&&f(l),d(i,t)}}}class N extends z{constructor(s){super(),C(this,s,null,G,L,{})}}export{N as default};
