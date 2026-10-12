// <copyright file="BoardContactDebugInfo.cs" company="Harris Hill Products Inc.">
//     Copyright (c) Harris Hill Products Inc. All rights reserved.
// </copyright>

namespace Board.Samples.Input
{
    using Board.Input;
    
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Provides a mechanism to display debug information about a <see cref="BoardContact"/>.
    /// </summary>
    public class BoardContactDebugInfo : MonoBehaviour
    {
        [SerializeField] private Text m_TouchLabel;
        [SerializeField] private Text m_GlyphLabel;
        [SerializeField] private RectTransform m_RotationIndicatorTransform;

        private RectTransform m_Transform;

        /// <summary>
        /// Callback invoked by Unity when the enabled <see cref="MonoBehaviour"/> is being loaded.
        /// </summary>
        private void Awake()
        {
            m_Transform = (RectTransform)transform;
        }

        /// <summary>
        /// Sets the position and orientation to match a specified <see cref="BoardContact"/>.
        /// </summary>
        /// <param name="contact">A <see cref="BoardContact"/></param>
        public void SetPositionAndRotation(BoardContact contact)
        {
            m_RotationIndicatorTransform.gameObject.SetActive(contact.type == BoardContactType.Glyph);
            m_TouchLabel.enabled = contact.type == BoardContactType.Finger;
            m_GlyphLabel.enabled = contact.type == BoardContactType.Glyph;
            m_Transform.position = contact.screenPosition;
            m_RotationIndicatorTransform.rotation =
                Quaternion.AngleAxis(contact.orientation * Mathf.Rad2Deg, Vector3.forward);
            
            if (contact.type == BoardContactType.Glyph)
            {
                m_GlyphLabel.text =
                    $"ID: {contact.contactId}\nGlyph: {contact.glyphId}\n{contact.screenPosition}\n{contact.orientation}";
            }
            else if (contact.type == BoardContactType.Finger)
            {
                m_TouchLabel.text = $"ID: {contact.contactId}\n{contact.screenPosition}";
            }
        }
    }
}