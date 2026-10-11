// <copyright file="BoardInputManager.cs" company="Harris Hill Products Inc.">
//     Copyright (c) Harris Hill Products Inc. All rights reserved.
// </copyright>

namespace Board.Samples.Input
{
    using System.Collections.Generic;
    using System.Text;
    
    using Board.Input;
    
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Demonstrates how to get input from the Board platform.
    /// </summary>
    public class BoardInputManager : MonoBehaviour
    {
        [SerializeField] private Canvas m_Canvas;
        [SerializeField] private BoardContactDebugInfo m_ContactDebugPrefab;
        
        [Header("Debug")] 
        [SerializeField] private Text m_TouchesDebugLabel;
        [SerializeField] private Text m_GlyphsDebugLabel;
        
        private readonly Dictionary<int, BoardContactDebugInfo> m_ContactDebugInstances = new Dictionary<int, BoardContactDebugInfo>();
        private StringBuilder m_DebugTextBuilder = new StringBuilder();
        
        /// <summary>
        /// Callback invoked when the <see cref="MonoBehaviour"/> updates.
        /// </summary>
        private void Update()
        {
            // Log out debug information for glyphs
            m_DebugTextBuilder.Clear();
            m_DebugTextBuilder.AppendLine("Glyphs");
            ProcessContacts(BoardInput.GetActiveContacts(BoardContactType.Glyph), m_GlyphsDebugLabel);

            // Log out debug information for touches
            m_DebugTextBuilder.Clear();
            m_DebugTextBuilder.AppendLine("Touches");
            ProcessContacts(BoardInput.GetActiveContacts(BoardContactType.Finger), m_TouchesDebugLabel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contacts">A list of contacts to process.</param>
        /// <param name="debugText"></param>
        private void ProcessContacts(BoardContact[] contacts, Text debugText)
        {
            // Iterate through the list of contacts 
            for (var i = 0; i < contacts.Length; i++)
            {
                BoardContactDebugInfo info;
                var contact = contacts[i];
                var position = contact.screenPosition;
                switch (contact.phase)
                {
                    case BoardContactPhase.Began:
                        // Make sure we haven't already made a debug game object for this contact
                        if (m_ContactDebugInstances.ContainsKey(contact.contactId))
                        {
                            return;
                        }
                
                        // Create a new debug info game object and assign this contact to it.
                        info = Instantiate(m_ContactDebugPrefab, m_Canvas.transform);
                        info.SetPositionAndRotation(contact);
                        ((RectTransform)info.transform).anchoredPosition = new Vector2(position.x, Screen.height - position.y);
                        m_ContactDebugInstances.Add(contact.contactId, info);
                        break;
                    case BoardContactPhase.Moved:
                    case BoardContactPhase.Stationary:
                        if (m_ContactDebugInstances.TryGetValue(contact.contactId, out info))
                        {
                            info.SetPositionAndRotation(contact);
                        }
                        break;
                    case BoardContactPhase.Canceled:
                    case BoardContactPhase.Ended:
                        // Find the debug info object associated with this contact
                        if (m_ContactDebugInstances.TryGetValue(contact.contactId, out info))
                        {
                            // Remove it from the dictionary and destroy it
                            m_ContactDebugInstances.Remove(contact.contactId);
                            Destroy(info.gameObject);
                        }
                        break;
                    default:
                        break;
                }

                if (contact.type == BoardContactType.Glyph)
                {
                    // Format a string for debug text to log to the screen
                    m_DebugTextBuilder.AppendLine($"{contact.contactId}: {contact.phase} {position} {contact.orientation} Glyph: {contact.glyphId}");
                }
                else if (contact.type == BoardContactType.Finger)
                {
                    // Format a string for debug text to log to the screen
                    m_DebugTextBuilder.AppendLine($"{contact.contactId}: {contact.phase} {position} {contact.orientation}");
                }
            }
        
            // Update the debug text label
            debugText.text = m_DebugTextBuilder.ToString();
        }
    }
}