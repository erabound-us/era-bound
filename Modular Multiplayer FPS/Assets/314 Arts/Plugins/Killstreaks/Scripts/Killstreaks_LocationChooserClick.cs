using UnityEngine;
using UnityEngine.EventSystems;

namespace KillstreaksPlugin
{
    public class Killstreaks_LocationChooserClick : MonoBehaviour, IPointerClickHandler
    {
        public RectTransform textureRectTransform;
        /// <summary>
        /// Render texture cam
        /// </summary>
        public Camera cam;
        /// <summary>
        /// Layers for world
        /// </summary>
        public LayerMask worldLayers;
        /// <summary>
        /// Location chooser
        /// </summary>
        public Killstreaks_LocationChooserModule locationChooser;

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 localClick;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(textureRectTransform, Input.mousePosition, null, out localClick);
            //This is the viewport of the Rect Transform and goes from -1, -1 to 1, 1
            Vector2 viewportClick = new Vector2(localClick.x / textureRectTransform.rect.xMax, localClick.y / (textureRectTransform.rect.yMin * -1));
            //Convert Texture Viewport to Camera viewport
            //Camera Bottom Left is 0,0
            Vector2 camViewPort = new Vector2(Mathf.Lerp(0f, 1f, (viewportClick.x + 1f) / 2f), Mathf.Lerp(0f, 1f, (viewportClick.y + 1f) / 2f));
            Vector3 worldClick = cam.ViewportToWorldPoint(camViewPort);

            RaycastHit hit;
            if (Physics.Raycast(worldClick, -Vector3.up, out hit, 20000, worldLayers.value, QueryTriggerInteraction.Ignore))
            {
                locationChooser.LocationWasPicked(hit.point);
            }
        }
    }
}