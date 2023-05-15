import{S as v,i as x,s as z,x as m,a as V,y as f,c as w,z as $,b as c,f as u,t as _,A as g,h as l,k as E,q as A,l as I,m as M,r as P,E as C,C as S}from"../../../chunks/index-86aa8d89.js";import{P as D}from"../../../chunks/PageTitle-cc378c43.js";import{S as b}from"../../../chunks/Section-4061cb73.js";import{D as R}from"../../../chunks/Diagram-a4d00bce.js";function T(o){let a,r;return{c(){a=E("p"),r=A(`The AuthorizationServer is built using Razor pages for UI,
supporting a MVVM architecture and a Vertical slice architecture
for endpoints with mediator.`)},l(e){a=I(e,"P",{});var n=M(a);r=P(n,`The AuthorizationServer is built using Razor pages for UI,
supporting a MVVM architecture and a Vertical slice architecture
for endpoints with mediator.`),n.forEach(l)},m(e,n){c(e,a,n),C(a,r)},p:S,d(e){e&&l(a)}}}function q(o){let a=`
        flowchart LR
        A(IEndpoint)
        B(IMediator)
        C(ValidatorPipeline)
        D(LoggingPipeline)
        E(Handler)
        A --> B --> C --> D --> E
    `,r;return{c(){r=A(a)},l(e){r=P(e,a)},m(e,n){c(e,r,n)},p:S,d(e){e&&l(r)}}}function B(o){let a,r,e,n,s,p;return a=new D({props:{title:"Architecture"}}),e=new b({props:{title:"Introduction",$$slots:{default:[T]},$$scope:{ctx:o}}}),s=new R({props:{$$slots:{default:[q]},$$scope:{ctx:o}}}),{c(){m(a.$$.fragment),r=V(),m(e.$$.fragment),n=V(),m(s.$$.fragment)},l(t){f(a.$$.fragment,t),r=w(t),f(e.$$.fragment,t),n=w(t),f(s.$$.fragment,t)},m(t,i){$(a,t,i),c(t,r,i),$(e,t,i),c(t,n,i),$(s,t,i),p=!0},p(t,[i]){const d={};i&1&&(d.$$scope={dirty:i,ctx:t}),e.$set(d);const h={};i&1&&(h.$$scope={dirty:i,ctx:t}),s.$set(h)},i(t){p||(u(a.$$.fragment,t),u(e.$$.fragment,t),u(s.$$.fragment,t),p=!0)},o(t){_(a.$$.fragment,t),_(e.$$.fragment,t),_(s.$$.fragment,t),p=!1},d(t){g(a,t),t&&l(r),g(e,t),t&&l(n),g(s,t)}}}class H extends v{constructor(a){super(),x(this,a,null,B,z,{})}}export{H as default};
