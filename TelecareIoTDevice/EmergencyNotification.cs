// ----------------------------------------------------------------------------
// <copyright file="EmergencyNotification" company="SOGETI Spain">
//     Copyright © 2015 SOGETI Spain. All rights reserved.
//     Connect Your IoT Device to the Cloud by Osc@rNET.
// </copyright>
// ----------------------------------------------------------------------------
namespace TelecareIoTDevice
{
    using System;

    /// <summary>
    /// Represents a emergency notification.
    /// </summary>
    internal sealed class EmergencyNotification
    {
        #region Properties

        /// <summary>
        /// Gets or sets when it was created.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> that represents when it was created.
        /// </value>
        public DateTime CreatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the full name of the person.
        /// </summary>
        /// <value>
        /// The full name of the person.
        /// </value>
        public string PersonFullName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public Guid PersonID
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "({0}) => PersonFullName = '{1}' / CreatedAt = '{2}' / PersonID = '{3}'",
                this.GetType().Name,
                this.PersonFullName,
                this.CreatedAt,
                this.PersonID);
        }

        #endregion Methods
    }
}
