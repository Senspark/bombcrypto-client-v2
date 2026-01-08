import {useLocalStorageState} from "ahooks";
import {useState, useEffect} from "react";
import {ethers} from "ethers";

import boardBG from '@assets/images/webgame polygon/boardrpc_bg.png';
import itemBG from '@assets/images/webgame polygon/itemrpc_bg.png';
import bnbIcon from '@assets/images/webgame polygon/icon_bnb.png';
import polygonIcon from '@assets/images/webgame polygon/icon_polygon.png';
import roninIcon from '@assets/images/webgame polygon/icon_ronin.png';
import baseIcon from '@assets/images/webgame polygon/icon_base.png';
import vicIcon from '@assets/images/webgame polygon/icon_vic.png';
import iconBG from "@assets/images/webgame polygon/icon_frame.png";
import greenBtn from "@assets/images/webgame polygon/green-button.png";
import styled from 'styled-components';

const FullScreenWrapper = styled.div<{ open: boolean }>`
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background-color: rgba(0, 0, 0, 0.5);
    display: ${({ open }) => (open ? 'flex' : 'none')};
    align-items: center;
    justify-content: center;
    z-index: 1001;
`;

const BoardWrapper = styled.div`
    position: relative;
    width: 513px; // Board width
    height: 266px; // Board height
    margin-bottom: 200px;
`;

const BoardBackground = styled.div`
    position: absolute;
    width: 100%;
    height: 100%;
    background-image: url(${boardBG});
    z-index: 0;
`;

const TitleText = styled.div`
    position: absolute;
    top: -43px;
    left: 50%;
    transform: translateX(-50%);
    color: white;
    font-size: 20px;
    font-weight: bold;
    text-shadow: 0 0 5px #27813f;
    z-index: 1;
`;

const ItemWrapper = styled.div`
    position: relative;
    top: 0px;
    display: flex;
    width: 90%;
    height: 70%;
    left: 50%;
    transform: translateX(-50%);
    flex-direction: column;
    gap: 5px;
    justify-content: end;
    align-items: center;
    z-index: 1;
`;

const ItemBackground = styled.div`
    width: 451px; // Item width
    height: 65px; // Item height
    background-image: url(${itemBG});
    background-size: cover;
    background-repeat: no-repeat;
    position: relative;
    display: flex;
    align-items: center;
    padding: 0 15px;
    border-radius: 8px;
    gap: 10px;
`;

const LeftBox = styled.div`
    flex: 0 0 50px; /* fixed width */
    display: flex;
    align-items: center;
    justify-content: center;
`;

const IconImage = styled.div`
    width: 49px;
    height: 45px;
    display: flex;
    align-items: center;
    justify-content: center;
    background-image: url(${iconBG});
`;

const RightBox = styled.div`
    flex: 1;
    display: flex;
    align-items: center;
`;

interface TextInputProps {
    hasError?: boolean;
}

const TextInput = styled.input<TextInputProps>`
    width: 100%;
    height: 35px;
    padding: 0 10px;
    color: #58333c;
    font-weight: bold;
    font-size: 20px;
    background-color: transparent;
    border: none;
    outline: none;
  
    &::placeholder {
      color: ${({ hasError }) => (hasError ? 'red' : '#58333c')};
      font-weight: bold;
      font-size: 20px;
    }
  `;

const ButtonWrapper = styled.div`
    position: absolute;
    bottom: 25px;
    left: 50%;
    margin-top: 10px;
    transform: translateX(-50%) scaleX(0.8) scaleY(0.8);
    display: flex;
    justify-content: center;
    align-items: center;
    width: 250px; // Button width
    height: 49px; // Button height
    background-image: url(${greenBtn});
    cursor: pointer;
    z-index: 1;
`;

const ButtonText = styled.span`
    font-size: 25px;
    font-weight: bold;
    text-shadow: 0 0 5px #27813f;
    margin-bottom: 5px;
`;

interface IProps {
    open: boolean;
    setOpen: (open: boolean) => void;
}

type StatusType = "" | "error" | "warning"
const RPC_BNB_DEFAULT_DESC = "RPC of BNB Network";
const RPC_POLYGON_DEFAULT_DESC = "RPC of POLYGON Network";
const RPC_RONIN_DEFAULT_DESC = "RPC of RONIN Network";
const RPC_BASE_DEFAULT_DESC = "RPC of BASE Network";
const RPC_VICTION_DEFAULT_DESC = "RPC of VICTION Network";
const BNB_NETWORK_ID = 56;
const POLYGON_NETWORK_ID = 137;
const RONIN_NETWORK_ID = 2020;
const BASE_NETWORK_ID = 8453;
const VICTION_NETWORK_ID = 88;

const Metamask = (props: IProps) => {
    const storageOptions = {
        serializer: (value: string | undefined) => value ?? ''
    };
    const [rpcBnb, setRpcBnb] = useLocalStorageState<string | undefined>('_rpc_url_bnb', storageOptions);
    const [rpcBnbError, setRpcBnbError] = useState<StatusType>("");
    const [rpcBnbDesc, setRpcBnbDesc] = useState<string>(RPC_BNB_DEFAULT_DESC);
    const [rpcBnbInput, setRpcBnbInput] = useState(rpcBnb || "");

    const [rpcPolygon, setRpcPolygon] = useLocalStorageState<string | undefined>('_rpc_url_polygon', storageOptions);
    const [rpcPolygonError, setRpcPolygonError] = useState<StatusType>("");
    const [rpcPolygonDesc, setRpcPolygonDesc] = useState<string>(RPC_POLYGON_DEFAULT_DESC);
    const [rpcPolygonInput, setRpcPolygonInput] = useState(rpcPolygon || "");

    const [rpcRonin, setRpcRonin] = useLocalStorageState<string | undefined>('_rpc_url_ronin', storageOptions);
    const [rpcRoninError, setRpcRoninError] = useState<StatusType>("");
    const [rpcRoninDesc, setRpcRoninDesc] = useState<string>(RPC_RONIN_DEFAULT_DESC);
    const [rpcRoninInput, setRpcRoninInput] = useState(rpcRonin || "");

    const [rpcBase, setRpcBase] = useLocalStorageState<string | undefined>('_rpc_url_base', storageOptions);
    const [rpcBaseError, setRpcBaseError] = useState<StatusType>("");
    const [rpcBaseDesc, setRpcBaseDesc] = useState<string>(RPC_BASE_DEFAULT_DESC);
    const [rpcBaseInput, setRpcBaseInput] = useState(rpcBase || "");

    const [rpcViction, setRpcViction] = useLocalStorageState<string | undefined>('_rpc_url_viction', storageOptions);
    const [rpcVictionError, setRpcVictionError] = useState<StatusType>("");
    const [rpcVictionDesc, setRpcVictionDesc] = useState<string>(RPC_VICTION_DEFAULT_DESC);
    const [rpcVictionInput, setRpcVictionInput] = useState(rpcViction || "");
    
    const [canClose, setCanClose] = useState<boolean>(true);

    //DevHoang: Add new airdrop
    const [ignoreBnb] = useState(true);
    const [ignorePolygon] = useState(true);
    const [ignoreRonin] = useState(false);
    const [ignoreBase] = useState(false);
    const [ignoreViction] = useState(false);

    const onModalOk = async () => {
        setCanClose(false);
        let isError = false;
        isError = await onModalOkBnb(ignoreBnb);
        isError = await onModalOkPolygon(ignorePolygon);
        isError = await onModalOkRonin(ignoreRonin);
        isError = await onModalOkBase(ignoreBase);
        isError = await onModalOkViction(ignoreViction);

        setCanClose(true);
        if (!isError && canClose) {
            onModalCancel();
        }
    };

    const onModalCancel = () => {
        if (canClose) {
            onModalCancelBnb(ignoreBnb);
            onModalCancelPolygon(ignorePolygon);
            onModalCancelRonin(ignoreRonin);
            onModalCancelBase(ignoreBase);
            onModalCancelViction(ignoreViction);

            props.setOpen(false);
        }
    };

    useEffect(() => {
        if (props.open) {
            setRpcInputBnb(ignoreBnb);
            setRpcInputPolygon(ignorePolygon);
            setRpcInputRonin(ignoreRonin);
            setRpcInputBase(ignoreBase);
            setRpcInputViction(ignoreViction);
        }
    }, [props.open]);

    const onModalOkBnb = async (triggered: boolean) => {
        if (!triggered) return false;

        if (rpcBnbInput.trim().length === 0) {
            setRpcBnbInput(rpcBnb || "");
            setRpcBnbDesc(RPC_BNB_DEFAULT_DESC);
        } 
        else if (await isValidUrl(rpcBnbInput, BNB_NETWORK_ID)) {
            setRpcBnbError("");
            if (rpcBnbInput.length === 0) {
                setRpcBnbDesc(RPC_BNB_DEFAULT_DESC);
            } else {
                setRpcBnb(rpcBnbInput);
            }
        } else {
            setRpcBnbError("error");
            setRpcBnbDesc(`Invalid ${RPC_BNB_DEFAULT_DESC}`);
            setRpcBnbInput("");
            return true;
        }
        return false;
    }

    const onModalOkPolygon = async (triggered: boolean) => {
        if (!triggered) return false;

        if (rpcPolygonInput.length === 0) {
            setRpcPolygonInput(rpcPolygon || "");
            setRpcPolygonDesc(RPC_POLYGON_DEFAULT_DESC);
        }
        else if (await isValidUrl(rpcPolygonInput, POLYGON_NETWORK_ID)) {
            setRpcPolygonError("");
            if (rpcPolygonInput.length === 0) {
                setRpcPolygonDesc(RPC_POLYGON_DEFAULT_DESC);
            } else {
                setRpcPolygon(rpcPolygonInput);
            }
        } else {
            setRpcPolygonError("error");
            setRpcPolygonDesc(`Invalid ${RPC_POLYGON_DEFAULT_DESC}`);
            setRpcPolygonInput("");
            return true;
        }
        return false;
    }

    const onModalOkRonin = async (triggered: boolean) => {
        if (!triggered) return false;

        if (rpcRoninInput.length === 0) {
            setRpcRoninInput(rpcRonin || "");
            setRpcRoninDesc(RPC_RONIN_DEFAULT_DESC);
        } 
        else if (await isValidUrl(rpcRoninInput, RONIN_NETWORK_ID)) {
            setRpcRoninError("");
            if (rpcRoninInput.length === 0) {
                setRpcRoninDesc(RPC_RONIN_DEFAULT_DESC);
            } else {
                setRpcRonin(rpcRoninInput);
            }
        } else {
            setRpcRoninError("error");
            setRpcRoninDesc(`Invalid ${RPC_RONIN_DEFAULT_DESC}`);
            setRpcRoninInput("");
            return true;
        }
        return false;
    }

    const onModalOkBase = async (triggered: boolean) => {
        if (!triggered) return false;

        if (rpcBaseInput.length === 0) {
            setRpcBaseInput(rpcBase || "");
            setRpcBaseDesc(RPC_BASE_DEFAULT_DESC);
        } 
        else if (await isValidUrl(rpcBaseInput, BASE_NETWORK_ID)) {
            setRpcBaseError("");
            if (rpcBaseInput.length === 0) {
                setRpcBaseDesc(RPC_BASE_DEFAULT_DESC);
            } else {
                setRpcBase(rpcBaseInput);
            }
        } else {
            setRpcBaseError("error");
            setRpcBaseDesc(`Invalid ${RPC_BASE_DEFAULT_DESC}`);
            setRpcBaseInput("");
            return true;
        }
        return false;
    }

    const onModalOkViction = async (triggered: boolean) => {
        if (!triggered) return false;

        if (rpcVictionInput.length === 0) {
            setRpcVictionInput(rpcViction || "");
            setRpcVictionDesc(RPC_VICTION_DEFAULT_DESC);
        } 
        else if (await isValidUrl(rpcVictionInput, VICTION_NETWORK_ID)) {
            setRpcVictionError("");
            if (rpcVictionInput.length === 0) {
                setRpcVictionDesc(RPC_VICTION_DEFAULT_DESC);
            } else {
                setRpcViction(rpcVictionInput);
            }
        } else {
            setRpcVictionError("error");
            setRpcVictionDesc(`Invalid ${RPC_VICTION_DEFAULT_DESC}`);
            setRpcVictionInput("");
            return true;
        }
        return false;
    }

    const onModalCancelBnb = (triggered: boolean) => {
        if (!triggered) return;

        setRpcBnbError("");
        setRpcBnbDesc(RPC_BNB_DEFAULT_DESC);
    }

    const onModalCancelPolygon = (triggered: boolean) => {
        if (!triggered) return;

        setRpcPolygonError("");
        setRpcPolygonDesc(RPC_POLYGON_DEFAULT_DESC);
    }

    const onModalCancelRonin = (triggered: boolean) => {
        if (!triggered) return;

        setRpcRoninError("");
        setRpcRoninDesc(RPC_RONIN_DEFAULT_DESC);
    }

    const onModalCancelBase = (triggered: boolean) => {
        if (!triggered) return;

        setRpcBaseError("");
        setRpcBaseDesc(RPC_BASE_DEFAULT_DESC);
    }

    const onModalCancelViction = (triggered: boolean) => {
        if (!triggered) return;

        setRpcVictionError("");
        setRpcVictionDesc(RPC_VICTION_DEFAULT_DESC);
    }

    const setRpcInputBnb = (triggered: boolean) => {
        if (!triggered) return;

        setRpcBnbInput(rpcBnb || "");
        setRpcBnbDesc(RPC_BNB_DEFAULT_DESC);
    }

    const setRpcInputPolygon = (triggered: boolean) => {
        if (!triggered) return;

        setRpcPolygonInput(rpcPolygon || "");
        setRpcPolygonDesc(RPC_POLYGON_DEFAULT_DESC);
    }

    const setRpcInputRonin = (triggered: boolean) => {
        if (!triggered) return;

        setRpcRoninInput(rpcRonin || "");
        setRpcRoninDesc(RPC_RONIN_DEFAULT_DESC);
    }

    const setRpcInputBase = (triggered: boolean) => {
        if (!triggered) return;

        setRpcBaseInput(rpcBase || "");
        setRpcBaseDesc(RPC_BASE_DEFAULT_DESC);
    }

    const setRpcInputViction = (triggered: boolean) => {
        if (!triggered) return;

        setRpcVictionInput(rpcViction || "");
        setRpcVictionDesc(RPC_VICTION_DEFAULT_DESC);
    }

    const handleContentClick = (e: React.MouseEvent) => {
        e.stopPropagation(); // ⛔️ Stops click from closing modal
    };

    const modal = (
        <FullScreenWrapper open={props.open} onClick={onModalCancel}>
            <BoardWrapper onClick={handleContentClick}>
                <TitleText>Custom RPC</TitleText>
                <BoardBackground/>
                <ItemWrapper>
                    {ignoreBnb ? null : (
                        <ItemBackground>
                            <LeftBox>
                                <IconImage>
                                    <img 
                                    src={bnbIcon}
                                    style={{ maxHeight: '70%', maxWidth: '70%' }}/>
                                </IconImage>
                            </LeftBox>
                            <RightBox>
                                <TextInput
                                    value={rpcBnbInput}
                                    onChange={(e) => setRpcBnbInput(e.target.value)}
                                    placeholder={rpcBnbDesc}
                                    hasError={rpcBnbError !== ""}
                                />
                            </RightBox>
                        </ItemBackground>
                    )}
                    {ignorePolygon ? null : (
                        <ItemBackground>
                            <LeftBox>
                                <IconImage>
                                    <img 
                                    src={polygonIcon}
                                    style={{ maxHeight: '70%', maxWidth: '70%' }}/>
                                </IconImage>
                            </LeftBox>
                            <RightBox>
                                <TextInput
                                    value={rpcPolygonInput}
                                    onChange={(e) => setRpcPolygonInput(e.target.value)}
                                    placeholder={rpcPolygonDesc}
                                    hasError={rpcPolygonError !== ""}
                                />
                            </RightBox>
                        </ItemBackground>
                    )}
                    {ignoreRonin ? null : (
                        <ItemBackground>
                            <LeftBox>
                                <IconImage>
                                    <img 
                                    src={roninIcon}
                                    style={{ maxHeight: '70%', maxWidth: '70%' }}/>
                                </IconImage>
                            </LeftBox>
                            <RightBox>
                                <TextInput
                                    value={rpcRoninInput}
                                    onChange={(e) => setRpcRoninInput(e.target.value)}
                                    placeholder={rpcRoninDesc}
                                    hasError={rpcRoninError !== ""}
                                />
                            </RightBox>
                        </ItemBackground>
                    )}
                    {ignoreBase ? null : (
                        <ItemBackground>
                            <LeftBox>
                                <IconImage>
                                    <img 
                                    src={baseIcon}
                                    style={{ maxHeight: '70%', maxWidth: '70%' }}/>
                                </IconImage>
                            </LeftBox>
                            <RightBox>
                                <TextInput
                                    value={rpcBaseInput}
                                    onChange={(e) => setRpcBaseInput(e.target.value)}
                                    placeholder={rpcBaseDesc}
                                    hasError={rpcBaseError !== ""}
                                />
                            </RightBox>
                        </ItemBackground>
                    )}
                    {ignoreViction ? null : (
                        <ItemBackground>
                            <LeftBox>
                                <IconImage>
                                    <img 
                                    src={vicIcon}
                                    style={{ maxHeight: '70%', maxWidth: '70%' }}/>
                                </IconImage>
                            </LeftBox>
                            <RightBox>
                                <TextInput
                                    value={rpcVictionInput}
                                    onChange={(e) => setRpcVictionInput(e.target.value)}
                                    placeholder={rpcVictionDesc}
                                    hasError={rpcVictionError !== ""}
                                />
                            </RightBox>
                        </ItemBackground>
                    )}
                </ItemWrapper>
                <ButtonWrapper onClick={onModalOk}>
                    <ButtonText>OK</ButtonText>
                </ButtonWrapper>
            </BoardWrapper>
        </FullScreenWrapper>
    );
    return (
        <>
            {modal}
        </>
    );
};

    async function isValidUrl(urlStr: string, networkId: number): Promise<boolean> {
        if (!urlStr || urlStr.length === 0) {
            return true; // allow empty
        }
        try {
            const provider = new ethers.JsonRpcProvider(urlStr);
            const blockNumber = await provider.getBlockNumber();
            if (blockNumber > 0) {
                const network = await provider.getNetwork();
                return network.chainId == ethers.toBigInt(networkId);
            }
            return false;
        } catch {
            return false;
        }
    }

    export default Metamask;