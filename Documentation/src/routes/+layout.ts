import type { MermaidConfig } from 'mermaid'
import mermaid from 'mermaid'

export const prerender = true;
export const trailingSlash = "always";
const mermaidConfig : MermaidConfig = {
    startOnLoad: true,
    theme: 'neutral',
    sequence: {
        mirrorActors: false
    }
}
mermaid.initialize(mermaidConfig);