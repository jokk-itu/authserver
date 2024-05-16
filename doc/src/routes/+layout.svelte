<script>
     import "../app.css";
     import { base } from "$app/paths";
</script>


<div class="flex flew-row divide-x-2">
     <div class="flex flex-col px-10 pt-4 flex-grow-0 flex-shrink-0 max-sm:hidden">
         <ul class="list-disc">
             <p class="text-2xl">AuthorizationServer</p>
             <hr class="pb-2"/>
             <li><a class="ref-link text-lg" href="{base}/">Introduction</a></li>
             <li><a class="ref-link text-lg" href="{base}/architecture">Architecture</a></li>
             <li><a class="ref-link text-lg" href="{base}/configurationapp">Configuration App</a></li>
             <li><a class="ref-link text-lg" href="{base}/authorize">Authorize</a></li>
             <li><a class="ref-link text-lg" href="{base}/token">Token</a></li>
             <li><a class="ref-link text-lg" href="{base}/datamodel">DataModel</a></li>
             <li>
               <a class="ref-link text-lg" href="{base}/dynamicclientregistration">Dynamic Client Registration</a>
             </li>
             <li>
               <a class="ref-link text-lg" href="{base}/logout">Logout</a>
               <ul class="pl-4 list-disc">
                    <li>
                         <a class="ref-link" href="{base}/logout/backchannel">Backchannel</a>
                    </li>
                    <li>
                         <a class="ref-link" href="{base}/logout/rpinitiated">RP Initiated</a>
                    </li>
               </ul>
             </li>
             <li><a class="ref-link text-lg" href="{base}/userinfo">UserInfo</a></li>
             <li><a class="ref-link text-lg" href="{base}/discovery">Discovery</a></li>
             <li><a class="ref-link text-lg" href="{base}/introspection">Introspection</a></li>
             <li><a class="ref-link text-lg" href="{base}/revocation">Revocation</a></li>
             <li>
               <a class="ref-link text-lg" href="{base}/clients">Clients</a>
               <ul class="pl-4 list-disc">
                    <li>
                         <a class="ref-link" href="{base}/clients/bffapp">BFFApp</a>
                    </li>
                    <li>
                         <a class="ref-link" href="{base}/clients/worker">Worker</a>
                    </li>
                    <li>
                         <a class="ref-link" href="{base}/clients/webapp">WebApp</a>
                    </li>
               </ul>
             </li>
             <li>
               <a class="ref-link text-lg" href="{base}/resources">Resources</a>
               <ul class="pl-4 list-disc">
                    <li>
                         <a class="ref-link" href="{base}/resources/weather">Weather</a>
                    </li>
               </ul>
             </li>
          </ul>
     </div>
 
     <div class="px-12 pt-4 flex-grow-3 w-screen overflow-clip">
         <slot/>
     </div>
 </div>