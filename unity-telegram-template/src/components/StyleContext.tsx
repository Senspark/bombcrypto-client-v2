import  { createContext, useState, ReactNode } from "react";

interface StyleContextProps {
    fullRatio: boolean;
    setFullRatio: (display: boolean) => void;
}

export const StyleContext = createContext<StyleContextProps | undefined>(undefined);

export const StyleProvider = ({ children }: { children: ReactNode }) => {
    const [wrapperDisplay, setWrapperDisplay] = useState(false);

    return (
        <StyleContext.Provider value={{ fullRatio: wrapperDisplay, setFullRatio: setWrapperDisplay }}>
            {children}
        </StyleContext.Provider>
    );
};