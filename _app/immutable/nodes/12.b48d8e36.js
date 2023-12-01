import{S as W,i as X,s as Y,y as D,a as g,k as v,z as O,c as d,l as P,A as E,b as o,g as I,d as C,B as w,h as i,q as S,m as q,r as B,D as m,H as N,n as T}from"../chunks/index.8cffed02.js";import{P as Z}from"../chunks/PageTitle.df959b80.js";import{S as H}from"../chunks/Section.be805b22.js";import{D as j}from"../chunks/Diagram.ad75e5b6.js";function y(l){let n,r;return{c(){n=v("p"),r=S("Used to create, get, update and delete clients.")},l(e){n=P(e,"P",{});var a=q(n);r=B(a,"Used to create, get, update and delete clients."),a.forEach(i)},m(e,a){o(e,n,a),m(n,r)},p:N,d(e){e&&i(n)}}}function ee(l){let n,r,e,a,$,U,f,A,b,h,c,k;return{c(){n=v("ul"),r=v("li"),e=v("a"),a=S("Dynamic Client Registration OIDC"),$=g(),U=v("li"),f=v("a"),A=S("Dynamic Client Registration Management OAuth"),b=g(),h=v("li"),c=v("a"),k=S("Dynamic Client Registration OAuth"),this.h()},l(R){n=P(R,"UL",{class:!0});var p=q(n);r=P(p,"LI",{});var u=q(r);e=P(u,"A",{href:!0});var L=q(e);a=B(L,"Dynamic Client Registration OIDC"),L.forEach(i),u.forEach(i),$=d(p),U=P(p,"LI",{});var G=q(U);f=P(G,"A",{href:!0});var M=q(f);A=B(M,"Dynamic Client Registration Management OAuth"),M.forEach(i),G.forEach(i),b=d(p),h=P(p,"LI",{});var _=q(h);c=P(_,"A",{href:!0});var z=q(c);k=B(z,"Dynamic Client Registration OAuth"),z.forEach(i),_.forEach(i),p.forEach(i),this.h()},h(){T(e,"href","https://openid.net/specs/openid-connect-registration-1_0.html"),T(f,"href","https://datatracker.ietf.org/doc/html/rfc7592"),T(c,"href","https://datatracker.ietf.org/doc/html/rfc7591"),T(n,"class","list-disc")},m(R,p){o(R,n,p),m(n,r),m(r,e),m(e,a),m(n,$),m(n,U),m(U,f),m(f,A),m(n,b),m(n,h),m(h,c),m(c,k)},p:N,d(R){R&&i(n)}}}function te(l){let n=`
sequenceDiagram
actor EndUser
participant OpenIDProvider as OP
EndUser->>OpenIDProvider: Post client registration endpoint
OpenIDProvider->>EndUser: Created 201 with client information
`,r;return{c(){r=S(n)},l(e){r=B(e,n)},m(e,a){o(e,r,a)},p:N,d(e){e&&i(r)}}}function ne(l){let n,r;return n=new j({props:{$$slots:{default:[te]},$$scope:{ctx:l}}}),{c(){D(n.$$.fragment)},l(e){O(n.$$.fragment,e)},m(e,a){E(n,e,a),r=!0},p(e,a){const $={};a&1&&($.$$scope={dirty:a,ctx:e}),n.$set($)},i(e){r||(I(n.$$.fragment,e),r=!0)},o(e){C(n.$$.fragment,e),r=!1},d(e){w(n,e)}}}function re(l){let n=`
sequenceDiagram
actor EndUser
participant OpenIDProvider as OP
EndUser->>OpenIDProvider: Put client configuration endpoint
OpenIDProvider->>EndUser: Ok 200 with client information
`,r;return{c(){r=S(n)},l(e){r=B(e,n)},m(e,a){o(e,r,a)},p:N,d(e){e&&i(r)}}}function ae(l){let n,r;return n=new j({props:{$$slots:{default:[re]},$$scope:{ctx:l}}}),{c(){D(n.$$.fragment)},l(e){O(n.$$.fragment,e)},m(e,a){E(n,e,a),r=!0},p(e,a){const $={};a&1&&($.$$scope={dirty:a,ctx:e}),n.$set($)},i(e){r||(I(n.$$.fragment,e),r=!0)},o(e){C(n.$$.fragment,e),r=!1},d(e){w(n,e)}}}function se(l){let n=`
sequenceDiagram
actor EndUser
participant OpenIDProvider as OP
EndUser->>OpenIDProvider: Get client endpoint with registration access token
OpenIDProvider->>EndUser: Ok 200 with client information
`,r;return{c(){r=S(n)},l(e){r=B(e,n)},m(e,a){o(e,r,a)},p:N,d(e){e&&i(r)}}}function ie(l){let n,r;return n=new j({props:{$$slots:{default:[se]},$$scope:{ctx:l}}}),{c(){D(n.$$.fragment)},l(e){O(n.$$.fragment,e)},m(e,a){E(n,e,a),r=!0},p(e,a){const $={};a&1&&($.$$scope={dirty:a,ctx:e}),n.$set($)},i(e){r||(I(n.$$.fragment,e),r=!0)},o(e){C(n.$$.fragment,e),r=!1},d(e){w(n,e)}}}function $e(l){let n=`
sequenceDiagram
actor EndUser
participant OpenIDProvider as OP
EndUser->>OpenIDProvider: Delete client endpoint with registration access token
OpenIDProvider->>EndUser: No Content 204
`,r;return{c(){r=S(n)},l(e){r=B(e,n)},m(e,a){o(e,r,a)},p:N,d(e){e&&i(r)}}}function le(l){let n,r;return n=new j({props:{$$slots:{default:[$e]},$$scope:{ctx:l}}}),{c(){D(n.$$.fragment)},l(e){O(n.$$.fragment,e)},m(e,a){E(n,e,a),r=!0},p(e,a){const $={};a&1&&($.$$scope={dirty:a,ctx:e}),n.$set($)},i(e){r||(I(n.$$.fragment,e),r=!0)},o(e){C(n.$$.fragment,e),r=!1},d(e){w(n,e)}}}function oe(l){let n,r,e,a,$,U,f,A,b,h,c,k,R,p,u,L,G,M,_,z;return n=new Z({props:{title:"Dynamic Client Registration"}}),e=new H({props:{title:"Introduction",$$slots:{default:[y]},$$scope:{ctx:l}}}),$=new H({props:{title:"Specifications",$$slots:{default:[ee]},$$scope:{ctx:l}}}),f=new H({props:{title:"Create a Client",$$slots:{default:[ne]},$$scope:{ctx:l}}}),c=new H({props:{title:"Update a Client",$$slots:{default:[ae]},$$scope:{ctx:l}}}),u=new H({props:{title:"Get a Client",$$slots:{default:[ie]},$$scope:{ctx:l}}}),_=new H({props:{title:"Deleting a Client",$$slots:{default:[le]},$$scope:{ctx:l}}}),{c(){D(n.$$.fragment),r=g(),D(e.$$.fragment),a=g(),D($.$$.fragment),U=g(),D(f.$$.fragment),A=g(),b=v("br"),h=g(),D(c.$$.fragment),k=g(),R=v("br"),p=g(),D(u.$$.fragment),L=g(),G=v("br"),M=g(),D(_.$$.fragment)},l(t){O(n.$$.fragment,t),r=d(t),O(e.$$.fragment,t),a=d(t),O($.$$.fragment,t),U=d(t),O(f.$$.fragment,t),A=d(t),b=P(t,"BR",{}),h=d(t),O(c.$$.fragment,t),k=d(t),R=P(t,"BR",{}),p=d(t),O(u.$$.fragment,t),L=d(t),G=P(t,"BR",{}),M=d(t),O(_.$$.fragment,t)},m(t,s){E(n,t,s),o(t,r,s),E(e,t,s),o(t,a,s),E($,t,s),o(t,U,s),E(f,t,s),o(t,A,s),o(t,b,s),o(t,h,s),E(c,t,s),o(t,k,s),o(t,R,s),o(t,p,s),E(u,t,s),o(t,L,s),o(t,G,s),o(t,M,s),E(_,t,s),z=!0},p(t,[s]){const x={};s&1&&(x.$$scope={dirty:s,ctx:t}),e.$set(x);const F={};s&1&&(F.$$scope={dirty:s,ctx:t}),$.$set(F);const J={};s&1&&(J.$$scope={dirty:s,ctx:t}),f.$set(J);const K={};s&1&&(K.$$scope={dirty:s,ctx:t}),c.$set(K);const Q={};s&1&&(Q.$$scope={dirty:s,ctx:t}),u.$set(Q);const V={};s&1&&(V.$$scope={dirty:s,ctx:t}),_.$set(V)},i(t){z||(I(n.$$.fragment,t),I(e.$$.fragment,t),I($.$$.fragment,t),I(f.$$.fragment,t),I(c.$$.fragment,t),I(u.$$.fragment,t),I(_.$$.fragment,t),z=!0)},o(t){C(n.$$.fragment,t),C(e.$$.fragment,t),C($.$$.fragment,t),C(f.$$.fragment,t),C(c.$$.fragment,t),C(u.$$.fragment,t),C(_.$$.fragment,t),z=!1},d(t){w(n,t),t&&i(r),w(e,t),t&&i(a),w($,t),t&&i(U),w(f,t),t&&i(A),t&&i(b),t&&i(h),w(c,t),t&&i(k),t&&i(R),t&&i(p),w(u,t),t&&i(L),t&&i(G),t&&i(M),w(_,t)}}}class ue extends W{constructor(n){super(),X(this,n,null,oe,Y,{})}}export{ue as component};
