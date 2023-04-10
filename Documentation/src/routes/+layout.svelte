<script>
     import "../app.css";
     import { base } from "$app/paths";
</script>

<div class="flex flew-row divide-x-2">
     <div class="flex flex-col px-10 pt-4 flex-grow-0">
         <p class="text-2xl">Authorization Server</p>
         <hr class="pb-4"/>
         <ul class="list-disc">
             <li><a class="ref-link text-lg" href="{base}/">Introduction</a></li>
             <li><a class="ref-link text-lg" href="{base}/authorize">Authorize</a></li>
             <li><a class="ref-link text-lg" href="{base}/token">Token</a></li>
             <li><a class="ref-link text-lg" href="{base}/datamodel">DataModel</a></li>
             <li>
               <a class="ref-link text-lg" href="{base}/grants">Grants</a>
               <ul class="pl-4 list-disc">
                    <li>
                         <a class="ref-link" href="{base}/grants/authorizationcode">
                              AuthorizationCode
                         </a>
                    </li>
                    <li>
                         <a class="ref-link" href="{base}/grants/clientcredentials">
                              Client Credentials
                         </a>
                    </li>
                    <li>
                         <a class="ref-link" href="{base}/grants/refreshtoken">
                              Refresh Token
                         </a>
                    </li>
               </ul>
             </li>
             <li>
               <a class="ref-link text-lg" href="{base}/dynamicregistration">Dynamic Registration</a>
               <ul class="pl-4 list-disc">
                    <li>
                         <a class="ref-link" href="{base}/dynamicregistration/client">Client</a>
                    </li>
                    <li>
                         <a class="ref-link" href="{base}/dynamicregistration/scope">Scope</a>
                    </li>
                    <li>
                         <a class="ref-link" href="{base}/dynamicregistration/resource">Resource</a>
                    </li>
               </ul>
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
             <li>
               <a class="ref-link text-lg" href="{base}/userinfo">UserInfo</a>
             </li>
             <li>
               <a class="ref-link text-lg" href="{base}/discovery">Discovery</a>
             </li>
             <li>
               <a class="ref-link text-lg" href="{base}/clients">Clients</a>
               <ul class="pl-4 list-disc">
                    <li>
                         <a class="ref-link" href="{base}/clients/blazor-bff">Blazor.BFF</a>
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
 
     <div class="px-12 pt-4 flex-grow-3">
         <slot/>
     </div>
 </div>