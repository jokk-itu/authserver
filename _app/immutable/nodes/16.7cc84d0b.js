import{S as je,i as qe,s as Ne,y as pe,a as R,z as be,c as O,A as _e,b as i,g as ae,d as de,B as me,h as t,k as o,q as p,l as n,m as T,r as b,D as $,H as ve,n as Le}from"../chunks/index.8cffed02.js";import{P as He}from"../chunks/PageTitle.df959b80.js";import{S as we}from"../chunks/Section.be805b22.js";function Ue(E){let r,_;return{c(){r=o("p"),_=p(`RP Initiated logout is used to initiate logout for the enduser.
It then triggers a session logout through backchannel logout.`)},l(f){r=n(f,"P",{});var d=T(r);_=b(d,`RP Initiated logout is used to initiate logout for the enduser.
It then triggers a session logout through backchannel logout.`),d.forEach(t)},m(f,d){i(f,r,d),$(r,_)},p:ve,d(f){f&&t(r)}}}function Je(E){let r,_,f,d;return{c(){r=o("ul"),_=o("li"),f=o("a"),d=p("RP Initiated Logout"),this.h()},l(u){r=n(u,"UL",{class:!0});var P=T(r);_=n(P,"LI",{});var m=T(_);f=n(m,"A",{href:!0});var k=T(f);d=b(k,"RP Initiated Logout"),k.forEach(t),m.forEach(t),P.forEach(t),this.h()},h(){Le(f,"href","https://openid.net/specs/openid-connect-rpinitiated-1_0.html"),Le(r,"class","list-disc")},m(u,P){i(u,r,P),$(r,_),$(_,f),$(f,d)},p:ve,d(u){u&&t(r)}}}function Me(E){let r,_,f,d,u,P,m,k,l,a,B,w,v,L,j,q,N,I,Pe,H,U,J,M,z,c,Te,C,D,F,K,Q,h,$e,V,W,X,Y,Z,y,ke,g,ee,te,ie,se,A,Re,le,oe,S,Oe,ne,re,fe,G,Ee,ue,x,Be;return{c(){r=p(`An endpoint is exposed at the OP, for the RP to initiate logout.
The endpoint is named end_session_endpoint.

The endpoint supports both GET and POST.
Parameters are serialized as a query string, if the RP uses GET.
Parameters are serialized as form url encoded, if the RP uses POST.

If GET is used, a page is returned allowing the enduser
to logout at the OP as well.

If POST is used, the enduser is logged out at the OP.

The following parameters are recognised:`),_=o("br"),f=o("br"),d=R(),u=o("b"),P=p("id_token_hint"),m=o("br"),k=p(`
RECOMMENDED. Used to identify the end-user at the OP.
The idToken will be validated on the following parameters:`),l=o("br"),a=p(`
iss is the OP.`),B=o("br"),w=p(`
exp is ignored.`),v=o("br"),L=p(`
sid must correspond to a session in the session table.`),j=o("br"),q=o("br"),N=R(),I=o("b"),Pe=p("logout_hint"),H=o("br"),U=p(`
OPTIONAL. Used to identify the enduser.
Useful if the user agent has multiple active sessions.
It can only be a session id.`),J=o("br"),M=o("br"),z=R(),c=o("b"),Te=p("client_id"),C=o("br"),D=p(`
OPTIONAL. Required if providing id_token_hint
or post_logout_redirect_uri.`),F=o("br"),K=o("br"),Q=R(),h=o("b"),$e=p("post_logout_redirect_uri"),V=o("br"),W=p(`
OPTIONAL. Used as a uri to redirect to after logout.
`),X=o("br"),Y=o("br"),Z=R(),y=o("b"),ke=p("state"),g=o("br"),ee=p(`
OPTIONAL. Required if the post_logout_redirect_uri is provided.
`),te=o("br"),ie=o("br"),se=R(),A=o("p"),Re=p("A POST example request can look like the following:"),le=o("br"),oe=R(),S=o("pre"),Oe=p(`POST /backchannel_logout HTTP/1.1
Host: https://webapp.authserver.dk/connect/end-session
Content-Type: application/x-www-form-urlencoded
      
id_token_hint=eyJhbGci.eyJpc3MiT3BlbklE&
logout_hint=ad5d2b25-4bd8-4edb-b075-3bafa0734f33&
client_id=bd6682a8-aa50-409b-9ae2-68841a356294&
post_logout_redirect_uri=https://webapp.authserver.dk&
state=fkoijbksdkbjfdj
`),ne=o("br"),re=o("br"),fe=R(),G=o("p"),Ee=p("A GET example request can look like the following:"),ue=R(),x=o("pre"),Be=p(`GET /backchannel_logout HTTP/1.1
Host: https://webapp.authserver.dk/connect/end-session?id_token_hint=eyJhbGci.eyJpc3MiT3BlbklE&logout_hint=ad5d2b25-4bd8-4edb-b075-3bafa0734f33&client_id=bd6682a8-aa50-409b-9ae2-68841a356294&post_logout_redirect_uri=https://webapp.authserver.dk&state=fkoijbksdkbjfdj
`)},l(e){r=b(e,`An endpoint is exposed at the OP, for the RP to initiate logout.
The endpoint is named end_session_endpoint.

The endpoint supports both GET and POST.
Parameters are serialized as a query string, if the RP uses GET.
Parameters are serialized as form url encoded, if the RP uses POST.

If GET is used, a page is returned allowing the enduser
to logout at the OP as well.

If POST is used, the enduser is logged out at the OP.

The following parameters are recognised:`),_=n(e,"BR",{}),f=n(e,"BR",{}),d=O(e),u=n(e,"B",{});var s=T(u);P=b(s,"id_token_hint"),s.forEach(t),m=n(e,"BR",{}),k=b(e,`
RECOMMENDED. Used to identify the end-user at the OP.
The idToken will be validated on the following parameters:`),l=n(e,"BR",{}),a=b(e,`
iss is the OP.`),B=n(e,"BR",{}),w=b(e,`
exp is ignored.`),v=n(e,"BR",{}),L=b(e,`
sid must correspond to a session in the session table.`),j=n(e,"BR",{}),q=n(e,"BR",{}),N=O(e),I=n(e,"B",{});var Ie=T(I);Pe=b(Ie,"logout_hint"),Ie.forEach(t),H=n(e,"BR",{}),U=b(e,`
OPTIONAL. Used to identify the enduser.
Useful if the user agent has multiple active sessions.
It can only be a session id.`),J=n(e,"BR",{}),M=n(e,"BR",{}),z=O(e),c=n(e,"B",{});var ce=T(c);Te=b(ce,"client_id"),ce.forEach(t),C=n(e,"BR",{}),D=b(e,`
OPTIONAL. Required if providing id_token_hint
or post_logout_redirect_uri.`),F=n(e,"BR",{}),K=n(e,"BR",{}),Q=O(e),h=n(e,"B",{});var he=T(h);$e=b(he,"post_logout_redirect_uri"),he.forEach(t),V=n(e,"BR",{}),W=b(e,`
OPTIONAL. Used as a uri to redirect to after logout.
`),X=n(e,"BR",{}),Y=n(e,"BR",{}),Z=O(e),y=n(e,"B",{});var ye=T(y);ke=b(ye,"state"),ye.forEach(t),g=n(e,"BR",{}),ee=b(e,`
OPTIONAL. Required if the post_logout_redirect_uri is provided.
`),te=n(e,"BR",{}),ie=n(e,"BR",{}),se=O(e),A=n(e,"P",{});var Ae=T(A);Re=b(Ae,"A POST example request can look like the following:"),Ae.forEach(t),le=n(e,"BR",{}),oe=O(e),S=n(e,"PRE",{});var Se=T(S);Oe=b(Se,`POST /backchannel_logout HTTP/1.1
Host: https://webapp.authserver.dk/connect/end-session
Content-Type: application/x-www-form-urlencoded
      
id_token_hint=eyJhbGci.eyJpc3MiT3BlbklE&
logout_hint=ad5d2b25-4bd8-4edb-b075-3bafa0734f33&
client_id=bd6682a8-aa50-409b-9ae2-68841a356294&
post_logout_redirect_uri=https://webapp.authserver.dk&
state=fkoijbksdkbjfdj
`),Se.forEach(t),ne=n(e,"BR",{}),re=n(e,"BR",{}),fe=O(e),G=n(e,"P",{});var Ge=T(G);Ee=b(Ge,"A GET example request can look like the following:"),Ge.forEach(t),ue=O(e),x=n(e,"PRE",{});var xe=T(x);Be=b(xe,`GET /backchannel_logout HTTP/1.1
Host: https://webapp.authserver.dk/connect/end-session?id_token_hint=eyJhbGci.eyJpc3MiT3BlbklE&logout_hint=ad5d2b25-4bd8-4edb-b075-3bafa0734f33&client_id=bd6682a8-aa50-409b-9ae2-68841a356294&post_logout_redirect_uri=https://webapp.authserver.dk&state=fkoijbksdkbjfdj
`),xe.forEach(t)},m(e,s){i(e,r,s),i(e,_,s),i(e,f,s),i(e,d,s),i(e,u,s),$(u,P),i(e,m,s),i(e,k,s),i(e,l,s),i(e,a,s),i(e,B,s),i(e,w,s),i(e,v,s),i(e,L,s),i(e,j,s),i(e,q,s),i(e,N,s),i(e,I,s),$(I,Pe),i(e,H,s),i(e,U,s),i(e,J,s),i(e,M,s),i(e,z,s),i(e,c,s),$(c,Te),i(e,C,s),i(e,D,s),i(e,F,s),i(e,K,s),i(e,Q,s),i(e,h,s),$(h,$e),i(e,V,s),i(e,W,s),i(e,X,s),i(e,Y,s),i(e,Z,s),i(e,y,s),$(y,ke),i(e,g,s),i(e,ee,s),i(e,te,s),i(e,ie,s),i(e,se,s),i(e,A,s),$(A,Re),i(e,le,s),i(e,oe,s),i(e,S,s),$(S,Oe),i(e,ne,s),i(e,re,s),i(e,fe,s),i(e,G,s),$(G,Ee),i(e,ue,s),i(e,x,s),$(x,Be)},p:ve,d(e){e&&t(r),e&&t(_),e&&t(f),e&&t(d),e&&t(u),e&&t(m),e&&t(k),e&&t(l),e&&t(a),e&&t(B),e&&t(w),e&&t(v),e&&t(L),e&&t(j),e&&t(q),e&&t(N),e&&t(I),e&&t(H),e&&t(U),e&&t(J),e&&t(M),e&&t(z),e&&t(c),e&&t(C),e&&t(D),e&&t(F),e&&t(K),e&&t(Q),e&&t(h),e&&t(V),e&&t(W),e&&t(X),e&&t(Y),e&&t(Z),e&&t(y),e&&t(g),e&&t(ee),e&&t(te),e&&t(ie),e&&t(se),e&&t(A),e&&t(le),e&&t(oe),e&&t(S),e&&t(ne),e&&t(re),e&&t(fe),e&&t(G),e&&t(ue),e&&t(x)}}}function ze(E){let r,_,f,d,u,P,m,k;return r=new He({props:{title:"RP Initiated Logout"}}),f=new we({props:{title:"Introduction",$$slots:{default:[Ue]},$$scope:{ctx:E}}}),u=new we({props:{title:"Specifications",$$slots:{default:[Je]},$$scope:{ctx:E}}}),m=new we({props:{title:"End Session",$$slots:{default:[Me]},$$scope:{ctx:E}}}),{c(){pe(r.$$.fragment),_=R(),pe(f.$$.fragment),d=R(),pe(u.$$.fragment),P=R(),pe(m.$$.fragment)},l(l){be(r.$$.fragment,l),_=O(l),be(f.$$.fragment,l),d=O(l),be(u.$$.fragment,l),P=O(l),be(m.$$.fragment,l)},m(l,a){_e(r,l,a),i(l,_,a),_e(f,l,a),i(l,d,a),_e(u,l,a),i(l,P,a),_e(m,l,a),k=!0},p(l,[a]){const B={};a&1&&(B.$$scope={dirty:a,ctx:l}),f.$set(B);const w={};a&1&&(w.$$scope={dirty:a,ctx:l}),u.$set(w);const v={};a&1&&(v.$$scope={dirty:a,ctx:l}),m.$set(v)},i(l){k||(ae(r.$$.fragment,l),ae(f.$$.fragment,l),ae(u.$$.fragment,l),ae(m.$$.fragment,l),k=!0)},o(l){de(r.$$.fragment,l),de(f.$$.fragment,l),de(u.$$.fragment,l),de(m.$$.fragment,l),k=!1},d(l){me(r,l),l&&t(_),me(f,l),l&&t(d),me(u,l),l&&t(P),me(m,l)}}}class Ke extends je{constructor(r){super(),qe(this,r,null,ze,Ne,{})}}export{Ke as component};
