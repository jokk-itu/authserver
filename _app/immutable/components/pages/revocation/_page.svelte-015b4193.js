import{S as ne,i as se,s as ie,c as j,a as y,b as U,d as B,m as G,k as s,o as J,p as N,q as Z,j as n,t as _,h as $,e as f,f as a,g as I,r as ee,l as P,n as te}from"../../../chunks/index-8ff2e5dd.js";import{P as oe}from"../../../chunks/PageTitle-edc21ece.js";import{S as X}from"../../../chunks/Section-732b932d.js";function le(d){let o;return{c(){o=_(`It is used to revoke a reference accesstoken or a refresh token. It is
used by clients when it wants to revoke a token, if it should not be
used any longer.`)},l(r){o=$(r,`It is used to revoke a reference accesstoken or a refresh token. It is
used by clients when it wants to revoke a token, if it should not be
used any longer.`)},m(r,l){s(r,o,l)},d(r){r&&n(o)}}}function re(d){let o,r,l,b;return{c(){o=f("ul"),r=f("li"),l=f("a"),b=_("OAuth Revocation"),this.h()},l(u){o=a(u,"UL",{class:!0});var m=I(o);r=a(m,"LI",{});var c=I(r);l=a(c,"A",{href:!0});var k=I(l);b=$(k,"OAuth Revocation"),k.forEach(n),c.forEach(n),m.forEach(n),this.h()},h(){ee(l,"href","https://www.rfc-editor.org/rfc/rfc7009"),ee(o,"class","list-disc")},m(u,m){s(u,o,m),P(o,r),P(r,l),P(l,b)},p:te,d(u){u&&n(o)}}}function fe(d){let o,r,l,b,u,m,c,k,t,p,v,h,w,E,R,K,S,z,O,A,q,F,x,T,V,C,D,H,L,M,Q,W;return{c(){o=_(`Requests are sent using the POST method,
and the body is encoded as form-urlencoded.
The client must also authenticate itself,
using an authentication method which is supported,
at the discovery endpoint atrevocation_endpoint_auth_methods_supported.`),r=f("br"),l=_(`
Access tokens can only be revoked if it is a reference token,
and Refresh tokens can always be revoked.`),b=f("br"),u=_(`
The following parameters are allowed:`),m=f("br"),c=y(),k=f("b"),t=_("token"),p=f("br"),v=_(`
REQUIRED. The token which should be revoked.
`),h=f("br"),w=f("br"),E=y(),R=f("b"),K=_("token_type_hint"),S=f("br"),z=_(`
OPTIONAL. The type of token, the client wants revoked.
The allowed values are: access_token and refresh_token
`),O=f("br"),A=f("br"),q=_(`
The request can look like the following using Basic client authentication:`),F=f("br"),x=y(),T=f("pre"),V=_(`    POST /connect/revoke HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    token=45ghiukldjahdnhzdauz&token_type_hint=refresh_token
`),C=f("br"),D=f("br"),H=_(`
If the requested token is successfully revoked or invalid,
the response contains a status code 200.`),L=f("br"),M=_(`
Invalid token_type_hints are ignored completetly.`),Q=f("br"),W=_(`
If client authentication fails, an invalid_client error is returned,
with status code 400.`)},l(e){o=$(e,`Requests are sent using the POST method,
and the body is encoded as form-urlencoded.
The client must also authenticate itself,
using an authentication method which is supported,
at the discovery endpoint atrevocation_endpoint_auth_methods_supported.`),r=a(e,"BR",{}),l=$(e,`
Access tokens can only be revoked if it is a reference token,
and Refresh tokens can always be revoked.`),b=a(e,"BR",{}),u=$(e,`
The following parameters are allowed:`),m=a(e,"BR",{}),c=B(e),k=a(e,"B",{});var i=I(k);t=$(i,"token"),i.forEach(n),p=a(e,"BR",{}),v=$(e,`
REQUIRED. The token which should be revoked.
`),h=a(e,"BR",{}),w=a(e,"BR",{}),E=B(e),R=a(e,"B",{});var Y=I(R);K=$(Y,"token_type_hint"),Y.forEach(n),S=a(e,"BR",{}),z=$(e,`
OPTIONAL. The type of token, the client wants revoked.
The allowed values are: access_token and refresh_token
`),O=a(e,"BR",{}),A=a(e,"BR",{}),q=$(e,`
The request can look like the following using Basic client authentication:`),F=a(e,"BR",{}),x=B(e),T=a(e,"PRE",{});var g=I(T);V=$(g,`    POST /connect/revoke HTTP/1.1
    Host: idp.authserver.dk
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    token=45ghiukldjahdnhzdauz&token_type_hint=refresh_token
`),g.forEach(n),C=a(e,"BR",{}),D=a(e,"BR",{}),H=$(e,`
If the requested token is successfully revoked or invalid,
the response contains a status code 200.`),L=a(e,"BR",{}),M=$(e,`
Invalid token_type_hints are ignored completetly.`),Q=a(e,"BR",{}),W=$(e,`
If client authentication fails, an invalid_client error is returned,
with status code 400.`)},m(e,i){s(e,o,i),s(e,r,i),s(e,l,i),s(e,b,i),s(e,u,i),s(e,m,i),s(e,c,i),s(e,k,i),P(k,t),s(e,p,i),s(e,v,i),s(e,h,i),s(e,w,i),s(e,E,i),s(e,R,i),P(R,K),s(e,S,i),s(e,z,i),s(e,O,i),s(e,A,i),s(e,q,i),s(e,F,i),s(e,x,i),s(e,T,i),P(T,V),s(e,C,i),s(e,D,i),s(e,H,i),s(e,L,i),s(e,M,i),s(e,Q,i),s(e,W,i)},p:te,d(e){e&&n(o),e&&n(r),e&&n(l),e&&n(b),e&&n(u),e&&n(m),e&&n(c),e&&n(k),e&&n(p),e&&n(v),e&&n(h),e&&n(w),e&&n(E),e&&n(R),e&&n(S),e&&n(z),e&&n(O),e&&n(A),e&&n(q),e&&n(F),e&&n(x),e&&n(T),e&&n(C),e&&n(D),e&&n(H),e&&n(L),e&&n(M),e&&n(Q),e&&n(W)}}}function ae(d){let o,r,l,b,u,m,c,k;return o=new oe({props:{title:"Revocation"}}),l=new X({props:{title:"Introduction",$$slots:{default:[le]},$$scope:{ctx:d}}}),u=new X({props:{title:"Specifications",$$slots:{default:[re]},$$scope:{ctx:d}}}),c=new X({props:{title:"Revocation",$$slots:{default:[fe]},$$scope:{ctx:d}}}),{c(){j(o.$$.fragment),r=y(),j(l.$$.fragment),b=y(),j(u.$$.fragment),m=y(),j(c.$$.fragment)},l(t){U(o.$$.fragment,t),r=B(t),U(l.$$.fragment,t),b=B(t),U(u.$$.fragment,t),m=B(t),U(c.$$.fragment,t)},m(t,p){G(o,t,p),s(t,r,p),G(l,t,p),s(t,b,p),G(u,t,p),s(t,m,p),G(c,t,p),k=!0},p(t,[p]){const v={};p&1&&(v.$$scope={dirty:p,ctx:t}),l.$set(v);const h={};p&1&&(h.$$scope={dirty:p,ctx:t}),u.$set(h);const w={};p&1&&(w.$$scope={dirty:p,ctx:t}),c.$set(w)},i(t){k||(J(o.$$.fragment,t),J(l.$$.fragment,t),J(u.$$.fragment,t),J(c.$$.fragment,t),k=!0)},o(t){N(o.$$.fragment,t),N(l.$$.fragment,t),N(u.$$.fragment,t),N(c.$$.fragment,t),k=!1},d(t){Z(o,t),t&&n(r),Z(l,t),t&&n(b),Z(u,t),t&&n(m),Z(c,t)}}}class me extends ne{constructor(o){super(),se(this,o,null,ae,ie,{})}}export{me as default};
