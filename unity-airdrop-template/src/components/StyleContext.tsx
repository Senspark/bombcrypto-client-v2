import  { createContext, useState, ReactNode } from "react";

interface StyleContextProps {
    fullratio: boolean;
    setfullratio: (display: boolean) => void;
}

export const StyleContext = createContext<StyleContextProps | undefined>(undefined);

export const StyleProvider = ({ children }: { children: ReactNode }) => {
    const [wrapperDisplay, setWrapperDisplay] = useState(false);

    return (
        <StyleContext.Provider value={{ fullratio: wrapperDisplay, setfullratio: setWrapperDisplay }}>
            {children}
        </StyleContext.Provider>
    );
};