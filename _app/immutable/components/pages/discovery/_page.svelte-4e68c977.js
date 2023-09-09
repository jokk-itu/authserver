import{S as As,i as Os,s as Ns,c,a as d,b as h,d as R,m as g,k as s,o as ee,p as te,q as se,j as t,t as o,h as f,e as r,f as l,g as _,r as Lt,l as b,n as ls}from"../../../chunks/index-8ff2e5dd.js";import{P as Ts}from"../../../chunks/PageTitle-edc21ece.js";import{S as Ut}from"../../../chunks/Section-732b932d.js";function zs(N){let u;return{c(){u=o(`Discovery is used for relying parties to fetch information
about the authorization server from a well known uri.
Information includes endpoints and supported features.`)},l(v){u=f(v,`Discovery is used for relying parties to fetch information
about the authorization server from a well known uri.
Information includes endpoints and supported features.`)},m(v,m){s(v,u,m)},d(v){v&&t(u)}}}function Ws(N){let u,v,m,y,$,w,n,k,J,S,p,B;return{c(){u=r("ul"),v=r("li"),m=r("a"),y=o("JWA"),$=d(),w=r("li"),n=r("a"),k=o("OAuth Discovery"),J=d(),S=r("li"),p=r("a"),B=o("OIDC Discovery"),this.h()},l(E){u=l(E,"UL",{class:!0});var a=_(u);v=l(a,"LI",{});var A=_(v);m=l(A,"A",{href:!0});var O=_(m);y=f(O,"JWA"),O.forEach(t),A.forEach(t),$=R(a),w=l(a,"LI",{});var z=_(w);n=l(z,"A",{href:!0});var T=_(n);k=f(T,"OAuth Discovery"),T.forEach(t),z.forEach(t),J=R(a),S=l(a,"LI",{});var x=_(S);p=l(x,"A",{href:!0});var W=_(p);B=f(W,"OIDC Discovery"),W.forEach(t),x.forEach(t),a.forEach(t),this.h()},h(){Lt(m,"href","https://datatracker.ietf.org/doc/html/rfc7518"),Lt(n,"href","https://datatracker.ietf.org/doc/html/rfc8414"),Lt(p,"href","https://openid.net/specs/openid-connect-discovery-1_0.html"),Lt(u,"class","list-disc")},m(E,a){s(E,u,a),b(u,v),b(v,m),b(m,y),b(u,$),b(u,w),b(w,n),b(n,k),b(u,J),b(u,S),b(S,p),b(p,B)},p:ls,d(E){E&&t(u)}}}function xs(N){let u,v,m,y,$,w,n,k,J,S,p,B,E,a,A,O,z,T,x,W,ie,re,le,pe,I,qt,oe,fe,ue,ne,be,j,Ft,_e,me,ve,Be,de,D,Mt,Re,ye,ke,$e,we,K,Qt,Je,ae,Se,Ee,Ae,C,Vt,Oe,Ne,Te,ze,We,P,Gt,xe,Ie,je,De,Ke,L,Ht,Ce,Pe,Le,Ue,qe,U,Xt,Fe,Me,Qe,Ve,Ge,q,Yt,He,Xe,Ye,Ze,ce,F,Zt,he,ge,et,tt,st,M,ct,it,rt,lt,pt,ot,Q,ht,ft,ut,nt,bt,_t,V,gt,mt,vt,Bt,dt,Rt,G,es,yt,kt,H,ts,$t,wt,Jt,at,St,X,ss,Et,At,Ot,Nt,Tt,Y,is,zt,Wt,xt,It,jt,Z,rs,Dt,Kt,Ct,Pt;return{c(){u=o(`An endpoint used to provide metadata about the authorization server. It exposes data such as endpoints,
supported grants and algorithms used for signing tokens.
The following is available in the metadata document:`),v=r("br"),m=d(),y=r("b"),$=o("issuer"),w=r("br"),n=o(`
The baseurl to the identity provider e.g: https://idp.authserver.dk
`),k=r("br"),J=r("br"),S=d(),p=r("b"),B=o("authorization_endpoint"),E=r("br"),a=o(`
Absolute url to the authorize endpoint e.g: https://idp.authserver.dk/connect/authorize
`),A=r("br"),O=r("br"),z=d(),T=r("b"),x=o("token_endpoint"),W=r("br"),ie=o(`
Absolute url to the token endpoint e.g: https://idp.authserver.dk/connect/token
`),re=r("br"),le=r("br"),pe=d(),I=r("b"),qt=o("userinfo_endpoint"),oe=r("br"),fe=o(`
Absolute url to the userinfo endpoint e.g: https://idp.authserver.dk/connect/userinfo
`),ue=r("br"),ne=r("br"),be=d(),j=r("b"),Ft=o("jwks_uri"),_e=r("br"),me=o(`
Absolute url to the jwks document e.g: https://idp.authserver.dk/.well-known/jwks
`),ve=r("br"),Be=r("br"),de=d(),D=r("b"),Mt=o("registration_endpoint"),Re=r("br"),ye=o(`
Absolute url to the registration endpoint e.g: https://idp.authserver.dk/connect/register
`),ke=r("br"),$e=r("br"),we=d(),K=r("b"),Qt=o("end_session_endpoint"),Je=r("br"),ae=o(`
Absolute url of the endsession endpoint e.g: https://idp.authserver.dk/connect/end-session
`),Se=r("br"),Ee=r("br"),Ae=d(),C=r("b"),Vt=o("scopes_supported"),Oe=r("br"),Ne=o(`
JSON array of supported scopes defined by the IdP.
For example openid and profile.
Custom scopes used by protected resources are not shown.
`),Te=r("br"),ze=r("br"),We=d(),P=r("b"),Gt=o("response_types_supported"),xe=r("br"),Ie=o(`
JSON array of supported response_type values,
used in the authorize endpoint. "code" is the only supported value.
`),je=r("br"),De=r("br"),Ke=d(),L=r("b"),Ht=o("response_modes_supported"),Ce=r("br"),Pe=o(`
JSON array of supported response_mode values,
used in the authorize endpoint. "form_post" is the only supported value.
`),Le=r("br"),Ue=r("br"),qe=d(),U=r("b"),Xt=o("grant_types_supported"),Fe=r("br"),Me=o(`
JSON array of supported grant_type values,
used in the token endpoint. "authorization_code", "client_credentials" and "refresh_token" are supported.
`),Qe=r("br"),Ve=r("br"),Ge=d(),q=r("b"),Yt=o("subject_types"),He=r("br"),Xe=o(`
JSON array of supported subject identifier types.
"public" is supported.
`),Ye=r("br"),Ze=r("br"),ce=d(),F=r("b"),Zt=o("id_token_signing_alg_values_supported"),he=r("br"),ge=o(`
JSON array of supported alg values used for signing id tokens.
"RS256" is supported.
`),et=r("br"),tt=r("br"),st=d(),M=r("b"),ct=o("token_endpoint_auth_methods_supported"),it=r("br"),rt=o(`
JSON array of supported client authentication methods at the token endpoint.
"client_secret_basic" and "client_secret_post" are supported.
`),lt=r("br"),pt=r("br"),ot=d(),Q=r("b"),ht=o("token_endpoint_auth_signing_alg_values_supported"),ft=r("br"),ut=o(`
JSON array of supported alg methods for signing tokens issued at the token endpoint.
"RS256" is supported.
`),nt=r("br"),bt=r("br"),_t=d(),V=r("b"),gt=o("revocation_endpoint"),mt=r("br"),vt=o(`
Absolute url to the revocation endpoint e.g: https://idp.authserver.dk/connect/revoke
`),Bt=r("br"),dt=r("br"),Rt=d(),G=r("b"),es=o("revocation_endpoint_auth_methods_supported"),yt=r("br"),kt=o(`
JSON array of supported client authentication methods at the revocation endpoint.
"client_secret_basic" and "client_secret_post" are supported.
`),H=r("b"),ts=o("introspection_endpoint"),$t=r("br"),wt=o(`
Absolute url to the introspection endpoint e.g: https://idp.authserver.dk/connect/introspect
`),Jt=r("br"),at=r("br"),St=d(),X=r("b"),ss=o("introspection_endpoint_auth_methods_supported"),Et=r("br"),At=o(`
JSON array of supported client authentication methods at the introspection endpoint.
"client_secret_basic" and "client_secret_post" are supported.
`),Ot=r("br"),Nt=r("br"),Tt=d(),Y=r("b"),is=o("code_challenge_methods_supported"),zt=r("br"),Wt=o(`
JSON array of supported alg values used for PKCE.
"R256" is supported.
`),xt=r("br"),It=r("br"),jt=d(),Z=r("b"),rs=o("claims_supported"),Dt=r("br"),Kt=o(`
JSON array of supported claims, which are found in id tokens and userinfo endpoint.
Claims include name, given_name, family_name, phone, email, address etc.
`),Ct=r("br"),Pt=r("br")},l(e){u=f(e,`An endpoint used to provide metadata about the authorization server. It exposes data such as endpoints,
supported grants and algorithms used for signing tokens.
The following is available in the metadata document:`),v=l(e,"BR",{}),m=R(e),y=l(e,"B",{});var i=_(y);$=f(i,"issuer"),i.forEach(t),w=l(e,"BR",{}),n=f(e,`
The baseurl to the identity provider e.g: https://idp.authserver.dk
`),k=l(e,"BR",{}),J=l(e,"BR",{}),S=R(e),p=l(e,"B",{});var ps=_(p);B=f(ps,"authorization_endpoint"),ps.forEach(t),E=l(e,"BR",{}),a=f(e,`
Absolute url to the authorize endpoint e.g: https://idp.authserver.dk/connect/authorize
`),A=l(e,"BR",{}),O=l(e,"BR",{}),z=R(e),T=l(e,"B",{});var os=_(T);x=f(os,"token_endpoint"),os.forEach(t),W=l(e,"BR",{}),ie=f(e,`
Absolute url to the token endpoint e.g: https://idp.authserver.dk/connect/token
`),re=l(e,"BR",{}),le=l(e,"BR",{}),pe=R(e),I=l(e,"B",{});var fs=_(I);qt=f(fs,"userinfo_endpoint"),fs.forEach(t),oe=l(e,"BR",{}),fe=f(e,`
Absolute url to the userinfo endpoint e.g: https://idp.authserver.dk/connect/userinfo
`),ue=l(e,"BR",{}),ne=l(e,"BR",{}),be=R(e),j=l(e,"B",{});var us=_(j);Ft=f(us,"jwks_uri"),us.forEach(t),_e=l(e,"BR",{}),me=f(e,`
Absolute url to the jwks document e.g: https://idp.authserver.dk/.well-known/jwks
`),ve=l(e,"BR",{}),Be=l(e,"BR",{}),de=R(e),D=l(e,"B",{});var ns=_(D);Mt=f(ns,"registration_endpoint"),ns.forEach(t),Re=l(e,"BR",{}),ye=f(e,`
Absolute url to the registration endpoint e.g: https://idp.authserver.dk/connect/register
`),ke=l(e,"BR",{}),$e=l(e,"BR",{}),we=R(e),K=l(e,"B",{});var bs=_(K);Qt=f(bs,"end_session_endpoint"),bs.forEach(t),Je=l(e,"BR",{}),ae=f(e,`
Absolute url of the endsession endpoint e.g: https://idp.authserver.dk/connect/end-session
`),Se=l(e,"BR",{}),Ee=l(e,"BR",{}),Ae=R(e),C=l(e,"B",{});var _s=_(C);Vt=f(_s,"scopes_supported"),_s.forEach(t),Oe=l(e,"BR",{}),Ne=f(e,`
JSON array of supported scopes defined by the IdP.
For example openid and profile.
Custom scopes used by protected resources are not shown.
`),Te=l(e,"BR",{}),ze=l(e,"BR",{}),We=R(e),P=l(e,"B",{});var ms=_(P);Gt=f(ms,"response_types_supported"),ms.forEach(t),xe=l(e,"BR",{}),Ie=f(e,`
JSON array of supported response_type values,
used in the authorize endpoint. "code" is the only supported value.
`),je=l(e,"BR",{}),De=l(e,"BR",{}),Ke=R(e),L=l(e,"B",{});var vs=_(L);Ht=f(vs,"response_modes_supported"),vs.forEach(t),Ce=l(e,"BR",{}),Pe=f(e,`
JSON array of supported response_mode values,
used in the authorize endpoint. "form_post" is the only supported value.
`),Le=l(e,"BR",{}),Ue=l(e,"BR",{}),qe=R(e),U=l(e,"B",{});var Bs=_(U);Xt=f(Bs,"grant_types_supported"),Bs.forEach(t),Fe=l(e,"BR",{}),Me=f(e,`
JSON array of supported grant_type values,
used in the token endpoint. "authorization_code", "client_credentials" and "refresh_token" are supported.
`),Qe=l(e,"BR",{}),Ve=l(e,"BR",{}),Ge=R(e),q=l(e,"B",{});var ds=_(q);Yt=f(ds,"subject_types"),ds.forEach(t),He=l(e,"BR",{}),Xe=f(e,`
JSON array of supported subject identifier types.
"public" is supported.
`),Ye=l(e,"BR",{}),Ze=l(e,"BR",{}),ce=R(e),F=l(e,"B",{});var Rs=_(F);Zt=f(Rs,"id_token_signing_alg_values_supported"),Rs.forEach(t),he=l(e,"BR",{}),ge=f(e,`
JSON array of supported alg values used for signing id tokens.
"RS256" is supported.
`),et=l(e,"BR",{}),tt=l(e,"BR",{}),st=R(e),M=l(e,"B",{});var ys=_(M);ct=f(ys,"token_endpoint_auth_methods_supported"),ys.forEach(t),it=l(e,"BR",{}),rt=f(e,`
JSON array of supported client authentication methods at the token endpoint.
"client_secret_basic" and "client_secret_post" are supported.
`),lt=l(e,"BR",{}),pt=l(e,"BR",{}),ot=R(e),Q=l(e,"B",{});var ks=_(Q);ht=f(ks,"token_endpoint_auth_signing_alg_values_supported"),ks.forEach(t),ft=l(e,"BR",{}),ut=f(e,`
JSON array of supported alg methods for signing tokens issued at the token endpoint.
"RS256" is supported.
`),nt=l(e,"BR",{}),bt=l(e,"BR",{}),_t=R(e),V=l(e,"B",{});var $s=_(V);gt=f($s,"revocation_endpoint"),$s.forEach(t),mt=l(e,"BR",{}),vt=f(e,`
Absolute url to the revocation endpoint e.g: https://idp.authserver.dk/connect/revoke
`),Bt=l(e,"BR",{}),dt=l(e,"BR",{}),Rt=R(e),G=l(e,"B",{});var ws=_(G);es=f(ws,"revocation_endpoint_auth_methods_supported"),ws.forEach(t),yt=l(e,"BR",{}),kt=f(e,`
JSON array of supported client authentication methods at the revocation endpoint.
"client_secret_basic" and "client_secret_post" are supported.
`),H=l(e,"B",{});var Js=_(H);ts=f(Js,"introspection_endpoint"),Js.forEach(t),$t=l(e,"BR",{}),wt=f(e,`
Absolute url to the introspection endpoint e.g: https://idp.authserver.dk/connect/introspect
`),Jt=l(e,"BR",{}),at=l(e,"BR",{}),St=R(e),X=l(e,"B",{});var as=_(X);ss=f(as,"introspection_endpoint_auth_methods_supported"),as.forEach(t),Et=l(e,"BR",{}),At=f(e,`
JSON array of supported client authentication methods at the introspection endpoint.
"client_secret_basic" and "client_secret_post" are supported.
`),Ot=l(e,"BR",{}),Nt=l(e,"BR",{}),Tt=R(e),Y=l(e,"B",{});var Ss=_(Y);is=f(Ss,"code_challenge_methods_supported"),Ss.forEach(t),zt=l(e,"BR",{}),Wt=f(e,`
JSON array of supported alg values used for PKCE.
"R256" is supported.
`),xt=l(e,"BR",{}),It=l(e,"BR",{}),jt=R(e),Z=l(e,"B",{});var Es=_(Z);rs=f(Es,"claims_supported"),Es.forEach(t),Dt=l(e,"BR",{}),Kt=f(e,`
JSON array of supported claims, which are found in id tokens and userinfo endpoint.
Claims include name, given_name, family_name, phone, email, address etc.
`),Ct=l(e,"BR",{}),Pt=l(e,"BR",{})},m(e,i){s(e,u,i),s(e,v,i),s(e,m,i),s(e,y,i),b(y,$),s(e,w,i),s(e,n,i),s(e,k,i),s(e,J,i),s(e,S,i),s(e,p,i),b(p,B),s(e,E,i),s(e,a,i),s(e,A,i),s(e,O,i),s(e,z,i),s(e,T,i),b(T,x),s(e,W,i),s(e,ie,i),s(e,re,i),s(e,le,i),s(e,pe,i),s(e,I,i),b(I,qt),s(e,oe,i),s(e,fe,i),s(e,ue,i),s(e,ne,i),s(e,be,i),s(e,j,i),b(j,Ft),s(e,_e,i),s(e,me,i),s(e,ve,i),s(e,Be,i),s(e,de,i),s(e,D,i),b(D,Mt),s(e,Re,i),s(e,ye,i),s(e,ke,i),s(e,$e,i),s(e,we,i),s(e,K,i),b(K,Qt),s(e,Je,i),s(e,ae,i),s(e,Se,i),s(e,Ee,i),s(e,Ae,i),s(e,C,i),b(C,Vt),s(e,Oe,i),s(e,Ne,i),s(e,Te,i),s(e,ze,i),s(e,We,i),s(e,P,i),b(P,Gt),s(e,xe,i),s(e,Ie,i),s(e,je,i),s(e,De,i),s(e,Ke,i),s(e,L,i),b(L,Ht),s(e,Ce,i),s(e,Pe,i),s(e,Le,i),s(e,Ue,i),s(e,qe,i),s(e,U,i),b(U,Xt),s(e,Fe,i),s(e,Me,i),s(e,Qe,i),s(e,Ve,i),s(e,Ge,i),s(e,q,i),b(q,Yt),s(e,He,i),s(e,Xe,i),s(e,Ye,i),s(e,Ze,i),s(e,ce,i),s(e,F,i),b(F,Zt),s(e,he,i),s(e,ge,i),s(e,et,i),s(e,tt,i),s(e,st,i),s(e,M,i),b(M,ct),s(e,it,i),s(e,rt,i),s(e,lt,i),s(e,pt,i),s(e,ot,i),s(e,Q,i),b(Q,ht),s(e,ft,i),s(e,ut,i),s(e,nt,i),s(e,bt,i),s(e,_t,i),s(e,V,i),b(V,gt),s(e,mt,i),s(e,vt,i),s(e,Bt,i),s(e,dt,i),s(e,Rt,i),s(e,G,i),b(G,es),s(e,yt,i),s(e,kt,i),s(e,H,i),b(H,ts),s(e,$t,i),s(e,wt,i),s(e,Jt,i),s(e,at,i),s(e,St,i),s(e,X,i),b(X,ss),s(e,Et,i),s(e,At,i),s(e,Ot,i),s(e,Nt,i),s(e,Tt,i),s(e,Y,i),b(Y,is),s(e,zt,i),s(e,Wt,i),s(e,xt,i),s(e,It,i),s(e,jt,i),s(e,Z,i),b(Z,rs),s(e,Dt,i),s(e,Kt,i),s(e,Ct,i),s(e,Pt,i)},p:ls,d(e){e&&t(u),e&&t(v),e&&t(m),e&&t(y),e&&t(w),e&&t(n),e&&t(k),e&&t(J),e&&t(S),e&&t(p),e&&t(E),e&&t(a),e&&t(A),e&&t(O),e&&t(z),e&&t(T),e&&t(W),e&&t(ie),e&&t(re),e&&t(le),e&&t(pe),e&&t(I),e&&t(oe),e&&t(fe),e&&t(ue),e&&t(ne),e&&t(be),e&&t(j),e&&t(_e),e&&t(me),e&&t(ve),e&&t(Be),e&&t(de),e&&t(D),e&&t(Re),e&&t(ye),e&&t(ke),e&&t($e),e&&t(we),e&&t(K),e&&t(Je),e&&t(ae),e&&t(Se),e&&t(Ee),e&&t(Ae),e&&t(C),e&&t(Oe),e&&t(Ne),e&&t(Te),e&&t(ze),e&&t(We),e&&t(P),e&&t(xe),e&&t(Ie),e&&t(je),e&&t(De),e&&t(Ke),e&&t(L),e&&t(Ce),e&&t(Pe),e&&t(Le),e&&t(Ue),e&&t(qe),e&&t(U),e&&t(Fe),e&&t(Me),e&&t(Qe),e&&t(Ve),e&&t(Ge),e&&t(q),e&&t(He),e&&t(Xe),e&&t(Ye),e&&t(Ze),e&&t(ce),e&&t(F),e&&t(he),e&&t(ge),e&&t(et),e&&t(tt),e&&t(st),e&&t(M),e&&t(it),e&&t(rt),e&&t(lt),e&&t(pt),e&&t(ot),e&&t(Q),e&&t(ft),e&&t(ut),e&&t(nt),e&&t(bt),e&&t(_t),e&&t(V),e&&t(mt),e&&t(vt),e&&t(Bt),e&&t(dt),e&&t(Rt),e&&t(G),e&&t(yt),e&&t(kt),e&&t(H),e&&t($t),e&&t(wt),e&&t(Jt),e&&t(at),e&&t(St),e&&t(X),e&&t(Et),e&&t(At),e&&t(Ot),e&&t(Nt),e&&t(Tt),e&&t(Y),e&&t(zt),e&&t(Wt),e&&t(xt),e&&t(It),e&&t(jt),e&&t(Z),e&&t(Dt),e&&t(Kt),e&&t(Ct),e&&t(Pt)}}}function Is(N){let u,v,m,y,$=`
{
    "kty": "RSA",
    "use": "sig",
    "kid": 1,
    "alg": "RS256",
    "n": "MODULUS VALUE",
    "e": "AQAB"
}
`,w;return{c(){u=o(`The array always returns three keys.
The first is the newest key which has expired.
This is important for resources which have received a JWT which is signed by an expired key.
The second JWK is the currently used for signing JWT.
The third JWK is the next in line, when the current JWK expires.`),v=r("br"),m=o(`
The format of a JWK is as follows:
`),y=r("pre"),w=o($)},l(n){u=f(n,`The array always returns three keys.
The first is the newest key which has expired.
This is important for resources which have received a JWT which is signed by an expired key.
The second JWK is the currently used for signing JWT.
The third JWK is the next in line, when the current JWK expires.`),v=l(n,"BR",{}),m=f(n,`
The format of a JWK is as follows:
`),y=l(n,"PRE",{});var k=_(y);w=f(k,$),k.forEach(t)},m(n,k){s(n,u,k),s(n,v,k),s(n,m,k),s(n,y,k),b(y,w)},p:ls,d(n){n&&t(u),n&&t(v),n&&t(m),n&&t(y)}}}function js(N){let u,v,m,y,$,w,n,k,J,S;return u=new Ts({props:{title:"Discovery Endpoint"}}),m=new Ut({props:{title:"Introduction",$$slots:{default:[zs]},$$scope:{ctx:N}}}),$=new Ut({props:{title:"Specifications",$$slots:{default:[Ws]},$$scope:{ctx:N}}}),n=new Ut({props:{title:"Discovery",$$slots:{default:[xs]},$$scope:{ctx:N}}}),J=new Ut({props:{title:"JWK",$$slots:{default:[Is]},$$scope:{ctx:N}}}),{c(){c(u.$$.fragment),v=d(),c(m.$$.fragment),y=d(),c($.$$.fragment),w=d(),c(n.$$.fragment),k=d(),c(J.$$.fragment)},l(p){h(u.$$.fragment,p),v=R(p),h(m.$$.fragment,p),y=R(p),h($.$$.fragment,p),w=R(p),h(n.$$.fragment,p),k=R(p),h(J.$$.fragment,p)},m(p,B){g(u,p,B),s(p,v,B),g(m,p,B),s(p,y,B),g($,p,B),s(p,w,B),g(n,p,B),s(p,k,B),g(J,p,B),S=!0},p(p,[B]){const E={};B&1&&(E.$$scope={dirty:B,ctx:p}),m.$set(E);const a={};B&1&&(a.$$scope={dirty:B,ctx:p}),$.$set(a);const A={};B&1&&(A.$$scope={dirty:B,ctx:p}),n.$set(A);const O={};B&1&&(O.$$scope={dirty:B,ctx:p}),J.$set(O)},i(p){S||(ee(u.$$.fragment,p),ee(m.$$.fragment,p),ee($.$$.fragment,p),ee(n.$$.fragment,p),ee(J.$$.fragment,p),S=!0)},o(p){te(u.$$.fragment,p),te(m.$$.fragment,p),te($.$$.fragment,p),te(n.$$.fragment,p),te(J.$$.fragment,p),S=!1},d(p){se(u,p),p&&t(v),se(m,p),p&&t(y),se($,p),p&&t(w),se(n,p),p&&t(k),se(J,p)}}}class Ps extends As{constructor(u){super(),Os(this,u,null,js,Ns,{})}}export{Ps as default};
