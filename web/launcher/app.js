let services=[],selected=0,columns=3,panelOpen=false;
const apps=document.getElementById("apps"),panel=document.getElementById("panel"),toast=document.getElementById("toast");
const post=o=>window.chrome?.webview?.postMessage(o);

function iconPath(value,id){
  const v=(value||"").trim();
  if(!v)return id?`https://bgftos.local/assets/services/${encodeURIComponent(id)}.svg`:"";
  if(/^https?:\/\//i.test(v))return v;
  if(/^[A-Za-z]:\\/.test(v)){
    const normalized=v.replaceAll("\\","/");
    const root="C:/TV/";
    if(normalized.toLowerCase().startsWith(root.toLowerCase()))
      return "https://bgftos.local/"+normalized.slice(root.length).split("/").map(encodeURIComponent).join("/");
    return "file:///"+normalized;
  }
  return "https://bgftos.local/"+v.replace(/^\.\.\//g,"").replace(/^\.\//,"").replace(/^\//,"").split("/").map(encodeURIComponent).join("/");
}

function render(){
  apps.innerHTML="";
  services.forEach((s,i)=>{
    const tile=document.createElement("div");
    tile.className="app"+(i===selected?" selected":"");
    tile.tabIndex=-1;tile.setAttribute("role","button");tile.setAttribute("aria-label",s.name);
    const img=document.createElement("img");
    img.src=iconPath(s.icon,s.id);img.alt=s.name;
    img.onerror=()=>{tile.innerHTML="";const f=document.createElement("div");f.className="fallback";f.textContent=s.name;tile.appendChild(f)};
    tile.appendChild(img);
    tile.onclick=()=>{selected=i;render();launch()};
    apps.appendChild(tile);
  });
}
function launch(){const s=services[selected];if(s)post({action:"launch",url:s.url})}
function openPanel(){panelOpen=true;panel.classList.remove("hidden");post({action:"remoteInfo"});document.getElementById("panelClose").focus()}
function closePanel(){panelOpen=false;panel.classList.add("hidden")}
function showToast(message){toast.textContent=message;toast.classList.add("show");setTimeout(()=>toast.classList.remove("show"),3200)}
function updateClock(){const d=new Date();document.getElementById("clock").textContent=d.toLocaleTimeString([],{hour:"numeric",minute:"2-digit"});document.getElementById("date").textContent=d.toLocaleDateString([],{weekday:"long",month:"short",day:"numeric"})}
window.chrome?.webview?.addEventListener("message",e=>{const m=e.data;if(m.type==="state"){services=m.services||[];selected=Math.min(selected,Math.max(0,services.length-1));document.body.dataset.theme=m.theme||"default";render()}else if(m.type==="remoteInfo"){document.getElementById("remoteUrl").textContent=m.url}else if(m.type==="mediaState"){renderMedia(m.items||[])}else if(m.type==="error")showToast(m.message||"Something went wrong")});
function renderMedia(items){const list=document.getElementById("mediaList");list.innerHTML="";if(!items.length){list.innerHTML='<div class="empty-library">No videos yet. Copy an MKV, MP4, M4V, VOB or other supported video into C:\\TV\\Media.</div>';return}items.forEach(item=>{const b=document.createElement("button");b.className="media-item focusable";b.textContent=item.title;b.onclick=()=>post({action:"playMedia",id:item.id});list.appendChild(b)})}
function openLibrary(){document.getElementById("libraryPanel").classList.remove("hidden");post({action:"mediaState"});document.getElementById("libraryClose").focus()}
function closeLibrary(){document.getElementById("libraryPanel").classList.add("hidden")}
document.getElementById("libraryButton").onclick=openLibrary;document.getElementById("libraryClose").onclick=closeLibrary;
document.getElementById("settings").onclick=openPanel;document.getElementById("brand").onclick=openPanel;document.getElementById("panelClose").onclick=closePanel;document.getElementById("sleepButton").onclick=()=>post({action:"sleep"});document.getElementById("refreshButton").onclick=()=>post({action:"getState"});
document.addEventListener("keydown",e=>{if(panelOpen){if(e.key==="Escape"||e.key==="Backspace"){e.preventDefault();closePanel()}return}if(!services.length)return;let handled=true;switch(e.key){case"ArrowRight":selected=Math.min(selected+1,services.length-1);break;case"ArrowLeft":selected=Math.max(selected-1,0);break;case"ArrowDown":selected=Math.min(selected+columns,services.length-1);break;case"ArrowUp":selected=Math.max(selected-columns,0);break;case"Enter":launch();return;default:handled=false}if(handled){e.preventDefault();render()}});
updateClock();setInterval(updateClock,15000);post({action:"getState"});
