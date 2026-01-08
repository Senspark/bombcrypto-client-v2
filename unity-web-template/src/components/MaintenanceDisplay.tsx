import React from "react";

export default function MaintenanceDisplay() {
    return (
        <div style={outerStyle}>
            <div style={warningStyle}>Server is under maintenance.</div>
        </div>
    );
}

const outerStyle: React.CSSProperties = {
    position: 'fixed',
    top: 0,
    left: 0,
    width: '100%',
    height: '100%',
    backgroundColor: 'black',
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
};

const warningStyle: React.CSSProperties = {
    color: '#F97068',
    fontSize: '2.5vw',
    fontWeight: 'bold',
    textAlign: 'center',
    backgroundColor: '#EDF2EF',
    padding: '60px',
    borderRadius: '8px',
    border: '1px solid #f5c6cb',
    boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
    maxWidth: '800px',
};