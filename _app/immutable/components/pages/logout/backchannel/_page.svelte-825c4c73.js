import{S as Le,i as ze,s as Ge,c as J,a as R,b as L,d as v,m as z,k as i,o as G,p as H,q as F,j as t,e as r,t as a,f,g as B,h as p,l as w,n as qe,r as Je}from"../../../../chunks/index-8ff2e5dd.js";import{P as He}from"../../../../chunks/PageTitle-edc21ece.js";import{S as Ie}from"../../../../chunks/Section-732b932d.js";import{D as Fe}from"../../../../chunks/Diagram-adaf7575.js";function Me(T){let n,u;return{c(){n=r("p"),u=a(`Functionality to send a request to a logout endpoint to all clients where a user has authorized access.
It allows for a single sign out functionality and it is more secure since it occurs through the backchannel.`)},l(o){n=f(o,"P",{});var b=B(n);u=p(b,`Functionality to send a request to a logout endpoint to all clients where a user has authorized access.
It allows for a single sign out functionality and it is more secure since it occurs through the backchannel.`),b.forEach(t)},m(o,b){i(o,n,b),w(n,u)},p:qe,d(o){o&&t(n)}}}function We(T){let n,u,o,b;return{c(){n=r("ul"),u=r("li"),o=r("a"),b=a("Backchannel Logout"),this.h()},l(m){n=f(m,"UL",{class:!0});var c=B(n);u=f(c,"LI",{});var _=B(u);o=f(_,"A",{href:!0});var k=B(o);b=p(k,"Backchannel Logout"),k.forEach(t),_.forEach(t),c.forEach(t),this.h()},h(){Je(o,"href","https://openid.net/specs/openid-connect-backchannel-1_0.html"),Je(n,"class","list-disc")},m(m,c){i(m,n,c),w(n,u),w(u,o),w(o,b)},p:qe,d(m){m&&t(n)}}}function Ae(T){let n=`
sequenceDiagram
participant RelyingParty as RP
participant OpenIDProvider as OP
OpenIDProvider->>RelyingParty: Requests backchannel logout uri
RelyingParty->>OpenIDProvider: Response 200
`,u;return{c(){u=a(n)},l(o){u=p(o,n)},m(o,b){i(o,u,b)},p:qe,d(o){o&&t(u)}}}function Ue(T){let n,u,o,b,m,c,_,k,s,$,P,y,E,M,I,Be,W,A,U,h,K,q,Re,N,Q,V,X,S,ve,Y,Z,d,g,ee,x,Te,te,le,ie,se,ne,D,Pe,re,fe,oe,ue,ae,O,ye,pe,be,me,$e,ce,_e,we,j,Ee,ke;return c=new Fe({props:{$$slots:{default:[Ae]},$$scope:{ctx:T}}}),{c(){n=r("p"),u=a(`The user logging out, will have its session identified.
Then for each active authorizationgrant in that session,
a client will be deduced.`),o=r("br"),b=a(`
Each client that has registered a backchannel_logout_uri
during registration, will receive a request.`),m=R(),J(c.$$.fragment),_=a(`
The request is sent using the POST method, and parameters
are encoded using application/x-www-for-urlencoded.
`),k=r("br"),s=a(`
It will contain a logout_token as a parameter.
The logout_token is a JWT token, which might be encrypted using
the registered method for id_token during client registration.
`),$=r("br"),P=r("br"),y=a(`
It contains the following claims:
`),E=r("br"),M=R(),I=r("b"),Be=a("iss"),W=r("br"),A=a(`
Issuer url.`),U=r("br"),h=r("br"),K=R(),q=r("b"),Re=a("sub"),N=a(`
Subject identifier.`),Q=r("br"),V=r("br"),X=R(),S=r("b"),ve=a("aud"),Y=r("br"),Z=a(`
ClientId.`),d=r("br"),g=r("br"),ee=R(),x=r("b"),Te=a("iat"),te=r("br"),le=a(`
Current DateTime.`),ie=r("br"),se=r("br"),ne=R(),D=r("b"),Pe=a("jti"),re=r("br"),fe=a(`
Id of token as a Guid.`),oe=r("br"),ue=r("br"),ae=R(),O=r("b"),ye=a("events"),pe=r("br"),be=a(`
Is an object with an empty property named http://schemas.openid.net/event/backchannel-logout
`),me=r("br"),$e=r("br"),ce=a(`
Example of a request can look like the following:`),_e=r("br"),we=R(),j=r("pre"),Ee=a(`POST /backchannel_logout HTTP/1.1
Host: https://webapp.authserver.dk
Content-Type: application/x-www-form-urlencoded
  
logout_token=eyJhbGci.eyJpc3MiT3BlbklE
`)},l(e){n=f(e,"P",{});var l=B(n);u=p(l,`The user logging out, will have its session identified.
Then for each active authorizationgrant in that session,
a client will be deduced.`),o=f(l,"BR",{}),b=p(l,`
Each client that has registered a backchannel_logout_uri
during registration, will receive a request.`),l.forEach(t),m=v(e),L(c.$$.fragment,e),_=p(e,`
The request is sent using the POST method, and parameters
are encoded using application/x-www-for-urlencoded.
`),k=f(e,"BR",{}),s=p(e,`
It will contain a logout_token as a parameter.
The logout_token is a JWT token, which might be encrypted using
the registered method for id_token during client registration.
`),$=f(e,"BR",{}),P=f(e,"BR",{}),y=p(e,`
It contains the following claims:
`),E=f(e,"BR",{}),M=v(e),I=f(e,"B",{});var C=B(I);Be=p(C,"iss"),C.forEach(t),W=f(e,"BR",{}),A=p(e,`
Issuer url.`),U=f(e,"BR",{}),h=f(e,"BR",{}),K=v(e),q=f(e,"B",{});var Se=B(q);Re=p(Se,"sub"),Se.forEach(t),N=p(e,`
Subject identifier.`),Q=f(e,"BR",{}),V=f(e,"BR",{}),X=v(e),S=f(e,"B",{});var xe=B(S);ve=p(xe,"aud"),xe.forEach(t),Y=f(e,"BR",{}),Z=p(e,`
ClientId.`),d=f(e,"BR",{}),g=f(e,"BR",{}),ee=v(e),x=f(e,"B",{});var De=B(x);Te=p(De,"iat"),De.forEach(t),te=f(e,"BR",{}),le=p(e,`
Current DateTime.`),ie=f(e,"BR",{}),se=f(e,"BR",{}),ne=v(e),D=f(e,"B",{});var Oe=B(D);Pe=p(Oe,"jti"),Oe.forEach(t),re=f(e,"BR",{}),fe=p(e,`
Id of token as a Guid.`),oe=f(e,"BR",{}),ue=f(e,"BR",{}),ae=v(e),O=f(e,"B",{});var je=B(O);ye=p(je,"events"),je.forEach(t),pe=f(e,"BR",{}),be=p(e,`
Is an object with an empty property named http://schemas.openid.net/event/backchannel-logout
`),me=f(e,"BR",{}),$e=f(e,"BR",{}),ce=p(e,`
Example of a request can look like the following:`),_e=f(e,"BR",{}),we=v(e),j=f(e,"PRE",{});var Ce=B(j);Ee=p(Ce,`POST /backchannel_logout HTTP/1.1
Host: https://webapp.authserver.dk
Content-Type: application/x-www-form-urlencoded
  
logout_token=eyJhbGci.eyJpc3MiT3BlbklE
`),Ce.forEach(t)},m(e,l){i(e,n,l),w(n,u),w(n,o),w(n,b),i(e,m,l),z(c,e,l),i(e,_,l),i(e,k,l),i(e,s,l),i(e,$,l),i(e,P,l),i(e,y,l),i(e,E,l),i(e,M,l),i(e,I,l),w(I,Be),i(e,W,l),i(e,A,l),i(e,U,l),i(e,h,l),i(e,K,l),i(e,q,l),w(q,Re),i(e,N,l),i(e,Q,l),i(e,V,l),i(e,X,l),i(e,S,l),w(S,ve),i(e,Y,l),i(e,Z,l),i(e,d,l),i(e,g,l),i(e,ee,l),i(e,x,l),w(x,Te),i(e,te,l),i(e,le,l),i(e,ie,l),i(e,se,l),i(e,ne,l),i(e,D,l),w(D,Pe),i(e,re,l),i(e,fe,l),i(e,oe,l),i(e,ue,l),i(e,ae,l),i(e,O,l),w(O,ye),i(e,pe,l),i(e,be,l),i(e,me,l),i(e,$e,l),i(e,ce,l),i(e,_e,l),i(e,we,l),i(e,j,l),w(j,Ee),ke=!0},p(e,l){const C={};l&1&&(C.$$scope={dirty:l,ctx:e}),c.$set(C)},i(e){ke||(G(c.$$.fragment,e),ke=!0)},o(e){H(c.$$.fragment,e),ke=!1},d(e){e&&t(n),e&&t(m),F(c,e),e&&t(_),e&&t(k),e&&t(s),e&&t($),e&&t(P),e&&t(y),e&&t(E),e&&t(M),e&&t(I),e&&t(W),e&&t(A),e&&t(U),e&&t(h),e&&t(K),e&&t(q),e&&t(N),e&&t(Q),e&&t(V),e&&t(X),e&&t(S),e&&t(Y),e&&t(Z),e&&t(d),e&&t(g),e&&t(ee),e&&t(x),e&&t(te),e&&t(le),e&&t(ie),e&&t(se),e&&t(ne),e&&t(D),e&&t(re),e&&t(fe),e&&t(oe),e&&t(ue),e&&t(ae),e&&t(O),e&&t(pe),e&&t(be),e&&t(me),e&&t($e),e&&t(ce),e&&t(_e),e&&t(we),e&&t(j)}}}function he(T){let n,u,o,b,m,c,_,k;return n=new He({props:{title:"Backchannel Logout"}}),o=new Ie({props:{title:"Introduction",$$slots:{default:[Me]},$$scope:{ctx:T}}}),m=new Ie({props:{title:"Specifications",$$slots:{default:[We]},$$scope:{ctx:T}}}),_=new Ie({props:{title:"Logout",$$slots:{default:[Ue]},$$scope:{ctx:T}}}),{c(){J(n.$$.fragment),u=R(),J(o.$$.fragment),b=R(),J(m.$$.fragment),c=R(),J(_.$$.fragment)},l(s){L(n.$$.fragment,s),u=v(s),L(o.$$.fragment,s),b=v(s),L(m.$$.fragment,s),c=v(s),L(_.$$.fragment,s)},m(s,$){z(n,s,$),i(s,u,$),z(o,s,$),i(s,b,$),z(m,s,$),i(s,c,$),z(_,s,$),k=!0},p(s,[$]){const P={};$&1&&(P.$$scope={dirty:$,ctx:s}),o.$set(P);const y={};$&1&&(y.$$scope={dirty:$,ctx:s}),m.$set(y);const E={};$&1&&(E.$$scope={dirty:$,ctx:s}),_.$set(E)},i(s){k||(G(n.$$.fragment,s),G(o.$$.fragment,s),G(m.$$.fragment,s),G(_.$$.fragment,s),k=!0)},o(s){H(n.$$.fragment,s),H(o.$$.fragment,s),H(m.$$.fragment,s),H(_.$$.fragment,s),k=!1},d(s){F(n,s),s&&t(u),F(o,s),s&&t(b),F(m,s),s&&t(c),F(_,s)}}}class Xe extends Le{constructor(n){super(),ze(this,n,null,he,Ge,{})}}export{Xe as default};
